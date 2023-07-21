using System;
using System.Collections.Generic;
using System.Linq;
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

		private List<int> _removeVisualizerCoordBuffer;
		private List<int> _waitBuildVisualizerCoords;
		private List<int> _buildingVisualizerCoords;

		private readonly List<Vector3Int> _visualizeLocalCoords = new();

		private int _currentCenterCoord = ChunkConstants.InvalidCoordId;

		public static readonly Dictionary<int, PackedTextureUvInfo> UvInfo = new();

		private static readonly bool[] EmptySolidMap = ArrayUtility.CreateArrayFilledWith(
			ChunkConstants.ChunkAxisCount * ChunkConstants.ChunkAxisCount * ChunkConstants.ChunkAxisCount,
			true
			);

		private const int MaxBuildingJobCount = 5;

		/// <summary>
		/// Coord -> ChunkComponent Entity
		/// </summary>
		private readonly Dictionary<int, int> _chunkEntities = new();

		private World _world;
		private Query<PlayerComponent, TransformComponent> _playerQuery;
		private Query<ChunkComponent> _chunkQuery;

		public void Init(World world)
		{
			_world = world;

			_playerQuery = new Query<PlayerComponent, TransformComponent>(world);
			_chunkQuery = new Query<ChunkComponent>(world);

			// 대략 구체에 가까운 모양새로 생성
			for (int x = -loadCoordDistance; x <= loadCoordDistance; x++)
			{
				for (int y = -loadCoordDistance; y <= loadCoordDistance; y++)
				{
					for (int z = -loadCoordDistance; z <= loadCoordDistance; z++)
					{
						if (x * x + y * y + z * z <= loadCoordDistance * loadCoordDistance)
						{
							_visualizeLocalCoords.Add(new Vector3Int(x, y, z));
						}
					}
				}
			}

			_visualizeLocalCoords.Sort((a, b) => a.sqrMagnitude.CompareTo(b.sqrMagnitude));

			// 동시에 사용 가능한 Coord 개수로 Add/Remove 버퍼 사이즈를 잡아줌
			var bufferSize = _visualizeLocalCoords.Count;

			_visualizers = new(bufferSize);

			_removeVisualizerCoordBuffer = new(bufferSize);
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

		public void LateUpdate()
		{
			// 월드가 생성되기 전까지는 동작하지 않도록
			if (_world == null)
			{
				return;
			}

			if (_chunkEntities.Count == 0)
			{
				foreach (var entity in _chunkQuery)
				{
					var chunkComponent = entity.Get<ChunkComponent>();

					_chunkEntities.Add(chunkComponent.coordId, entity.Id);
				}
			}

			if (_chunkEntities.Count == 0)
			{
				return;
			}

			foreach (var entity in _playerQuery)
			{
				var transformComponent = entity.Get<TransformComponent>();
				var curPos = transformComponent.Position;
				var prevCenterCoord = _currentCenterCoord;

				_currentCenterCoord = ChunkUtility.GetCoordId(
					ChunkUtility.GetCoordAxis(curPos.x),
					ChunkUtility.GetCoordAxis(curPos.y),
					ChunkUtility.GetCoordAxis(curPos.z)
					);

				// Remove
				if (_currentCenterCoord != prevCenterCoord)
				{
					Debug.Log($"Player moved Chunk : from {ChunkUtility.ConvertIdToPos(prevCenterCoord)} to {ChunkUtility.ConvertIdToPos(_currentCenterCoord)}");

					foreach (var keyValue in _visualizers)
					{
						var coord = keyValue.Key;

						if (ChunkUtility.GetCoordSqrDistance(_currentCenterCoord, coord) > loadCoordDistance * loadCoordDistance)
						{
							_removeVisualizerCoordBuffer.Add(coord);
						}
					}

					foreach (var coord in _removeVisualizerCoordBuffer)
					{
						RemoveVisualizer(coord);
					}

					_removeVisualizerCoordBuffer.Clear();

					foreach (var localOffset in _visualizeLocalCoords)
					{
						// 해당 좌표가 청크 범위를 넘어서지 않을 때 & Visualize되어있지 않은 경우만 체크
						if (ChunkUtility.TryMoveCoord(_currentCenterCoord, localOffset.x, localOffset.y, localOffset.z,
							    out var movedCoordId) && !_visualizers.ContainsKey(movedCoordId))
						{
							AddVisualizer(movedCoordId);

							for (int i = 0; i < VoxelConstants.BlockSideCount; i++)
							{
								var nearOffset = VoxelConstants.NearVoxels[i];

								if (ChunkUtility.TryMoveCoord(movedCoordId,
									    nearOffset.x, nearOffset.y, nearOffset.z, out var nearCoordId))
								{
									if (_visualizers.TryGetValue(nearCoordId, out var nearChunkBuilder) &&
									    _chunkEntities.TryGetValue(nearCoordId, out var entityId))
									{
										var chunkEntity = new Entity(_world, entityId);

										if (chunkEntity.IsAlive && chunkEntity.Has<ChunkComponent>())
										{
											var chunkComponent = chunkEntity.Get<ChunkComponent>();

											if (!chunkComponent.isEmpty)
											{
												if (!_waitBuildVisualizerCoords.Contains(nearCoordId))
												{
													_waitBuildVisualizerCoords.Add(nearCoordId);
												}

												nearChunkBuilder.StopBuildMesh();

												// 이미 Building 중이면 취소
												_buildingVisualizerCoords.Remove(nearCoordId);
											}
										}
									}
								}
							}
						}
					}

					_waitBuildVisualizerCoords.Sort((a, b) =>
					{
						var toASqr = ChunkUtility.GetCoordSqrDistance(_currentCenterCoord, a);
						var toBSqr = ChunkUtility.GetCoordSqrDistance(_currentCenterCoord, b);

						return toASqr.CompareTo(toBSqr);
					});
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
						var entityId = _chunkEntities[coord];
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

				// FIXME
				break;
			}
		}

		private bool[] GetSolidMap(int coord)
		{
			if (_chunkEntities.TryGetValue(coord, out var entityId))
			{
				var chunkEntity = new Entity(_world, entityId);

				if (chunkEntity.IsAlive && chunkEntity.Has<ChunkComponent>())
				{
					var chunkComponent = chunkEntity.Get<ChunkComponent>();

					return chunkComponent.isSolidMap;
				}
			}

			return EmptySolidMap;
		}

		private void AddVisualizer(int coord)
		{
			if (_visualizers.ContainsKey(coord))
			{
				Debug.LogError($"Cannot add ChunkVisualizer at same coord : { ChunkUtility.ConvertIdToPos(coord) }");
				return;
			}

			if (!ServiceManager.TryGetService<IObjectPoolService>(out var objectPoolService))
			{
				return;
			}

			var spawnCoordPos = new Vector3(ChunkUtility.GetCoordX(coord), ChunkUtility.GetCoordY(coord), ChunkUtility.GetCoordZ(coord));
			var spawnPos = spawnCoordPos * ChunkConstants.ChunkAxisCount;

			var visualizerGo = objectPoolService.Spawn(_visualizerPoolGuid, spawnPos);
			var visualizer = visualizerGo.GetComponent<ChunkVisualizer>();

			visualizer.gameObject.name = $"ChunkVisualizer ({ChunkUtility.GetCoordX(coord)}, {ChunkUtility.GetCoordY(coord)}, {ChunkUtility.GetCoordZ(coord)})";

			_visualizers.Add(coord, visualizer);

			visualizer.Initialize();

			if (_chunkEntities.TryGetValue(coord, out var entityId))
			{
				var chunkEntity = new Entity(_world, entityId);

				if (chunkEntity.IsAlive && chunkEntity.Has<ChunkComponent>())
				{
					var chunkComponent = chunkEntity.Get<ChunkComponent>();

					if (!chunkComponent.isEmpty)
					{
						_waitBuildVisualizerCoords.Add(coord);
					}
				}
			}
		}

		private void RemoveVisualizer(int coord)
		{
			if (!_visualizers.TryGetValue(coord, out var visualizer))
			{
				Debug.LogError($"Has no ChunkVisualizer to Remove at coord : { ChunkUtility.ConvertIdToPos(coord) }.");
				return;
			}

			visualizer.Dispose();
			_visualizers.Remove(coord);

			_waitBuildVisualizerCoords.Remove(coord);
			_buildingVisualizerCoords.Remove(coord);

			if (!ServiceManager.TryGetService<IObjectPoolService>(out var objectPoolService))
			{
				return;
			}

			objectPoolService.Despawn(visualizer.gameObject);
		}
	}
}
