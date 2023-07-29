using System;
using System.Collections.Generic;
using BlitzEcs;
using Core.Utility;
using Game;
using Game.Asset;
using Game.Ecs.Component;
using Game.World.Stage;
using Service;
using Service.ObjectPool;
using Service.Rendering;
using Service.Texture;
using UnityEngine;
using UnityService.Texture;

namespace UnityService.Rendering
{
	/// <summary>
	/// 여기서는 청크의 인덱스 좌표를 Coord로 사용
	/// </summary>
	[UnityService(typeof(IChunkService))]
	public class ChunkService : MonoBehaviour, IChunkService
	{
		[SerializeField]
		private string visualizerPoolGuid;

		[SerializeField]
		private int loadCoordDistance = 3;

		private Guid _visualizerPoolGuid;

		private Dictionary<int, ChunkVisualizer> _visualizers;

		private List<int> _waitBuildVisualizerCoords;
		private List<int> _buildingVisualizerCoords;

		public static readonly Dictionary<int, PackedTextureUvInfo> UvInfo = new();

		private static readonly bool[] EmptySolidMap = ArrayUtility.CreateArrayFilledWith(
			ChunkConstants.ChunkAxisCount * ChunkConstants.ChunkAxisCount * ChunkConstants.ChunkAxisCount,
			true
			);

		private const int MaxBuildingJobCount = 5;

		public int CoordViewDistance => loadCoordDistance;

		private bool _needSortWaitBuild = false;

		private World _world;

		private Query<ChunkComponent> _chunkQuery;

		private Query<PlayerComponent, TransformComponent> _playerQuery;

		private readonly Dictionary<int, Entity> _chunkEntityBuffer = new();

		public void Init(World world)
		{
			_world = world;
			_chunkQuery = new Query<ChunkComponent>(_world);
			_playerQuery = new Query<PlayerComponent, TransformComponent>(_world);

			var bufferSize = 0;

			// 동시에 사용 가능한 Coord 개수로 Add/Remove 버퍼 사이즈를 잡아줌
			for (int x = -loadCoordDistance; x <= loadCoordDistance; x++)
			{
				for (int y = -loadCoordDistance; y <= loadCoordDistance; y++)
				{
					for (int z = -loadCoordDistance; z <= loadCoordDistance; z++)
					{
						if (x * x + y * y + z * z <= loadCoordDistance * loadCoordDistance)
						{
							bufferSize++;
						}
					}
				}
			}

			_visualizers = new(bufferSize);

			_waitBuildVisualizerCoords = new(bufferSize);
			_buildingVisualizerCoords = new(MaxBuildingJobCount);

			_visualizerPoolGuid = new Guid(visualizerPoolGuid);

			if (AssetFactory.Instance.TryGetAssetReader<ScriptableAssetModule>(out var assetModule))
			{
				if (assetModule.TryGet<BlockSpecHolder>("BlockSpecs", out var blockSpecHolder) &&
				    assetModule.TryGet<PackedTextureUvHolder>("PackedTextureUvs", out var uvHolder))
				{
					var nameToUvInfo = new Dictionary<string, PackedTextureUvInfo>(uvHolder.uvs.Count);

					foreach (var uv in uvHolder.uvs)
					{
						nameToUvInfo.Add(uv.originTexture.name, new PackedTextureUvInfo
						{
							startX = (float)uv.startX / uvHolder.textureSize,
							startY = (float)uv.startY / uvHolder.textureSize,
							width = (float)uv.originTexture.width / uvHolder.textureSize,
							height = (float)uv.originTexture.height / uvHolder.textureSize,
						});
					}

					foreach (var blockSpec in blockSpecHolder.blockSpecs)
					{
						var blockUvs = new PackedTextureUvInfo[blockSpec.textures.Length];

						for (int i = 0; i < blockUvs.Length; i++)
						{
							var textureName = blockSpec.textures[i].name;

							if (nameToUvInfo.TryGetValue(textureName, out var info))
							{
								blockUvs[i] = info;

								UvInfo.Add((blockSpec.blockId << 3) | i, info);
							}
							else
							{
								Debug.LogError($"Has no texture [{textureName}] in PackedTextureUvHolder[{uvHolder.name}].");
							}
						}
					}

					nameToUvInfo.Clear();
				}
			}
		}

		public void StartFetch()
		{
			// 청크들을 캐싱해둘 순 없으니, 매번 버퍼를 갱신해둠
			foreach (var chunkEntity in _chunkQuery)
			{
				var chunkComponent = chunkEntity.Get<ChunkComponent>();

				_chunkEntityBuffer.Add(chunkComponent.coordId, chunkEntity);
			}
		}

		public void EndFetch()
		{
			// 신규 시각화 요청이 존재하여, 목록을 다시 정렬할 필요가 있다면
			if (_needSortWaitBuild)
			{
				var currentPlayerCoord = ChunkConstants.InvalidCoordId;

				foreach (var playerEntity in _playerQuery)
				{
					if (playerEntity.Get<PlayerComponent>().PlayerType != PlayerType.Local)
					{
						continue;
					}

					var transformComponent = playerEntity.Get<TransformComponent>();
					var curPos = transformComponent.Position;

					currentPlayerCoord = ChunkUtility.GetCoordId(
						ChunkUtility.GetCoordAxis(curPos.x),
						ChunkUtility.GetCoordAxis(curPos.y),
						ChunkUtility.GetCoordAxis(curPos.z)
					);

					break;
				}

				if (currentPlayerCoord != ChunkConstants.InvalidCoordId)
				{
					// 플레이어와
					_waitBuildVisualizerCoords.Sort((a, b) =>
					{
						var toASqr = ChunkUtility.GetCoordSqrDistance(currentPlayerCoord, a);
						var toBSqr = ChunkUtility.GetCoordSqrDistance(currentPlayerCoord, b);

						return toASqr.CompareTo(toBSqr);
					});

					_needSortWaitBuild = false;
				}
			}

			// Streaming
			for (int i = 0; i < _waitBuildVisualizerCoords.Count; i++)
			{
				var coord = _waitBuildVisualizerCoords[i];

				if (_buildingVisualizerCoords.Count == MaxBuildingJobCount)
				{
					break;
				}

				if (_visualizers.TryGetValue(coord, out var visualizer))
				{
					var entityId = _chunkEntityBuffer[coord];
					var chunkEntity = new Entity(_world, entityId);

					if (chunkEntity.IsAlive && chunkEntity.Has<ChunkComponent>())
					{
						var chunkComponent = chunkEntity.Get<ChunkComponent>();

						visualizer.RebuildMesh(
							chunkComponent.blockIdMap,
							GetSolidMap(coord),
							GetSolidMap(coord + ChunkConstants.NearCoordAdders[0]),
							GetSolidMap(coord + ChunkConstants.NearCoordAdders[1]),
							GetSolidMap(coord + ChunkConstants.NearCoordAdders[2]),
							GetSolidMap(coord + ChunkConstants.NearCoordAdders[3]),
							GetSolidMap(coord + ChunkConstants.NearCoordAdders[4]),
							GetSolidMap(coord + ChunkConstants.NearCoordAdders[5])
						);

						_waitBuildVisualizerCoords.RemoveAt(i--);

						_buildingVisualizerCoords.Add(coord);
					}
					else
					{
						Debug.LogError($"Cannot find ChunkComponent at {coord}.");
						_waitBuildVisualizerCoords.RemoveAt(i--);
					}
				}
				else
				{
					_waitBuildVisualizerCoords.RemoveAt(i--);
				}
			}

			// Check Build Done
			for (int i = 0; i < _buildingVisualizerCoords.Count; i++)
			{
				var coord = _buildingVisualizerCoords[i];

				if (_visualizers.TryGetValue(coord, out var visualizer) && visualizer.IsBuilding)
				{
					visualizer.UpdateBuildMesh();

					if (!visualizer.IsBuilding)
					{
						_buildingVisualizerCoords.RemoveAt(i--);
					}
				}
				else
				{
					// 모종의 이유로 청크 목록에서 사라졌다면 로그 출력
					Debug.LogError($"Building ChunkVisualizer at { ChunkUtility.ConvertIdToPos(coord) } disappeared.");

					_buildingVisualizerCoords.RemoveAt(i--);
				}
			}

			_chunkEntityBuffer.Clear();
		}

		private bool[] GetSolidMap(int coord)
		{
			if (_chunkEntityBuffer.TryGetValue(coord, out var chunkEntity))
			{
				if (chunkEntity.IsAlive && chunkEntity.Has<ChunkComponent>())
				{
					var chunkComponent = chunkEntity.Get<ChunkComponent>();

					return chunkComponent.isSolidMap;
				}
			}

			return EmptySolidMap;
		}

		private bool TryGetCoord(Entity chunkEntity, out int coordId)
		{
			if (chunkEntity.Has<ChunkComponent>())
			{
				var chunkComponent = chunkEntity.Get<ChunkComponent>();

				coordId = chunkComponent.coordId;

				return true;
			}

			coordId = ChunkConstants.InvalidCoordId;

			return false;
		}

		public void AddVisualizer(Entity entity)
		{
			if (!TryGetCoord(entity, out var coordId) || _visualizers.ContainsKey(coordId))
			{
				Debug.LogError($"Cannot add ChunkVisualizer at same coord : { ChunkUtility.ConvertIdToPos(coordId) }");
				return;
			}

			if (!ServiceManager.TryGetService<IObjectPoolService>(out var objectPoolService))
			{
				return;
			}

			Vector3 spawnCoordPos = ChunkUtility.ConvertIdToPos(coordId);
			var spawnPos = spawnCoordPos * ChunkConstants.ChunkAxisCount;

			var visualizerGo = objectPoolService.Spawn(_visualizerPoolGuid, spawnPos);
			var visualizer = visualizerGo.GetComponent<ChunkVisualizer>();

			visualizer.gameObject.name = $"ChunkVisualizer ({ChunkUtility.GetCoordX(coordId)}, {ChunkUtility.GetCoordY(coordId)}, {ChunkUtility.GetCoordZ(coordId)})";

			_visualizers.Add(coordId, visualizer);

			visualizer.Initialize();

			_waitBuildVisualizerCoords.Add(coordId);

			_needSortWaitBuild = true;
		}

		public void RemoveVisualizer(Entity entity)
		{
			if (!TryGetCoord(entity, out var coordId) || !_visualizers.TryGetValue(coordId, out var visualizer))
			{
				Debug.LogError($"Has no ChunkVisualizer to Remove at coord : { ChunkUtility.ConvertIdToPos(coordId) }.");
				return;
			}

			visualizer.Dispose();
			_visualizers.Remove(coordId);

			_waitBuildVisualizerCoords.Remove(coordId);
			_buildingVisualizerCoords.Remove(coordId);

			if (ServiceManager.TryGetService<IObjectPoolService>(out var objectPoolService))
			{
				objectPoolService.Despawn(visualizer.gameObject);
			}
		}

		public void UpdateVisualizer(Entity entity)
		{
			if (!TryGetCoord(entity, out var coordId))
			{
				return;
			}

			if (!_visualizers.TryGetValue(coordId, out var visualizer))
			{
				Debug.LogWarning($"Has no ChunkVisualizer at coord : { ChunkUtility.ConvertIdToPos(coordId) }.");

				AddVisualizer(entity);

				visualizer = _visualizers[coordId];
			}

			if (!_waitBuildVisualizerCoords.Contains(coordId))
			{
				_waitBuildVisualizerCoords.Add(coordId);

				_needSortWaitBuild = true;
			}

			visualizer.StopBuildMesh();

			// 이미 Building 중이면 취소
			_buildingVisualizerCoords.Remove(coordId);
		}
	}
}
