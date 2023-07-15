using System;
using System.Collections.Generic;
using System.Linq;
using BlitzEcs;
using Core.Utility;
using Game;
using Game.Asset;
using Game.Ecs.Component;
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
		private string chunkPoolGuid;

		[SerializeField]
		private int loadCoordDistance = 3;

		private Guid _chunkPoolGuid;

		private World _world;
		private Query<PlayerComponent, TransformComponent> _playerQuery;

		private Dictionary<int, Chunk> _chunks;

		private List<int> _removeChunkCoordBuffer;
		private List<int> _waitBuildChunkCoords;
		private List<int> _buildingChunkCoords;

		private readonly List<Vector3Int> _chunkLocalCoords = new();

		private int _currentCenterCoord = VoxelConstants.InvalidCoordId;

		public static readonly Dictionary<int, PackedTextureUvInfo> UvInfo = new();

		private static readonly bool[] EmptySolidMap = ArrayUtility.CreateArrayFilledWith(
			VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount,
			true
			);

		private const int MaxMeshBuildingJobCount = 5;

		private class ChunkData
		{
			public bool[] IsSolidMap { get; private set; }
			public int[] BlockIdMap  { get; private set; }

			private int _chunkType;

			public bool NeedBuild => _chunkType != -1;

			// FIXME : 임시
			public ChunkData(int chunkType)
			{
				_chunkType = chunkType;

				var capacity = VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount;

				BlockIdMap = new int[capacity];
				IsSolidMap = new bool[capacity];

				for (int y = 0; y < VoxelConstants.ChunkAxisCount; y++)
				{
					var yWeight = y << VoxelConstants.ChunkAxisExponent;

					for (int x = 0; x < VoxelConstants.ChunkAxisCount; x++)
					{
						var xWeight = x << (VoxelConstants.ChunkAxisExponent << 1);

						for (int z = 0; z < VoxelConstants.ChunkAxisCount; z++)
						{
							var index = xWeight | yWeight | z;

							if (chunkType == 1 && y < VoxelConstants.ChunkAxisCount - 1)
							{
								BlockIdMap[index] = 2;
							}
							else
							{
								BlockIdMap[index] = chunkType;
							}

							IsSolidMap[index] = BlockIdMap[index] >= 0;
						}
					}
				}
			}
		}

		private readonly Dictionary<int, ChunkData> _chunkData = new();

		public void Init(World world)
		{
			_world = world;

			_playerQuery = new Query<PlayerComponent, TransformComponent>(world);

			// 대략 구체에 가까운 모양새로 생성
			for (int x = -loadCoordDistance; x <= loadCoordDistance; x++)
			{
				for (int y = -loadCoordDistance; y <= loadCoordDistance; y++)
				{
					for (int z = -loadCoordDistance; z <= loadCoordDistance; z++)
					{
						if (x * x + y * y + z * z <= loadCoordDistance * loadCoordDistance)
						{
							_chunkLocalCoords.Add(new Vector3Int(x, y, z));
						}
					}
				}
			}

			_chunkLocalCoords.Sort((a, b) => a.sqrMagnitude.CompareTo(b.sqrMagnitude));

			// 동시에 사용 가능한 Coord 개수로 Add/Remove 버퍼 사이즈를 잡아줌
			var bufferSize = _chunkLocalCoords.Count;

			_chunks = new(bufferSize);

			_removeChunkCoordBuffer = new(bufferSize);
			_waitBuildChunkCoords = new(bufferSize);
			_buildingChunkCoords = new(MaxMeshBuildingJobCount);

			_chunkPoolGuid = new Guid(chunkPoolGuid);

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

			foreach (var entity in _playerQuery)
			{
				var transformComponent = entity.Get<TransformComponent>();
				var curPos = transformComponent.Position;
				var prevCenterCoord = _currentCenterCoord;

				_currentCenterCoord = VoxelUtility.GetCoordId(
					VoxelUtility.GetCoordAxis(curPos.x),
					VoxelUtility.GetCoordAxis(curPos.y),
					VoxelUtility.GetCoordAxis(curPos.z)
					);

				// Remove
				if (_currentCenterCoord != prevCenterCoord)
				{
					Debug.Log($"Player moved Chunk : from {VoxelUtility.ConvertIdToPos(prevCenterCoord)} to {VoxelUtility.ConvertIdToPos(_currentCenterCoord)}");

					foreach (var keyValue in _chunks)
					{
						var coord = keyValue.Key;

						if (VoxelUtility.GetCoordSqrDistance(_currentCenterCoord, coord) > loadCoordDistance * loadCoordDistance)
						{
							_removeChunkCoordBuffer.Add(coord);
						}
					}

					foreach (var coord in _removeChunkCoordBuffer)
					{
						RemoveChunk(coord);
					}

					_removeChunkCoordBuffer.Clear();

					foreach (var localOffset in _chunkLocalCoords)
					{
						// 해당 좌표가 청크 범위를 넘어서지 않을 때 & Visualize되어있지 않은 경우만 체크
						if (VoxelUtility.TryMoveCoord(_currentCenterCoord, localOffset.x, localOffset.y, localOffset.z,
							    out var movedCoordId) && !_chunks.ContainsKey(movedCoordId))
						{
							AddChunk(movedCoordId);

							for (int i = 0; i < VoxelConstants.BlockSideCount; i++)
							{
								var nearOffset = VoxelConstants.NearVoxels[i];

								if (VoxelUtility.TryMoveCoord(movedCoordId,
									    nearOffset.x, nearOffset.y, nearOffset.z, out var nearCoordId))
								{
									if (_chunks.TryGetValue(nearCoordId, out var nearChunk) &&
									    _chunkData.TryGetValue(nearCoordId, out var data) && data.NeedBuild)
									{
										if (!_waitBuildChunkCoords.Contains(nearCoordId))
										{
											_waitBuildChunkCoords.Add(nearCoordId);
										}

										nearChunk.StopBuildMesh();

										// 이미 Building 중이면 취소
										_buildingChunkCoords.Remove(nearCoordId);
									}
								}
							}
						}
					}

					_waitBuildChunkCoords.Sort((a, b) =>
					{
						var toASqr = VoxelUtility.GetCoordSqrDistance(_currentCenterCoord, a);
						var toBSqr = VoxelUtility.GetCoordSqrDistance(_currentCenterCoord, b);

						return toASqr.CompareTo(toBSqr);
					});
				}

				// Streaming
				for (int i = 0; i < _waitBuildChunkCoords.Count; i++)
				{
					var coord = _waitBuildChunkCoords[i];

					if (_buildingChunkCoords.Count == MaxMeshBuildingJobCount)
					{
						break;
					}

					if (_chunks.TryGetValue(coord, out var chunk))
					{
						var blockIdMap = _chunkData[coord].BlockIdMap;

						chunk.RebuildMesh(
							blockIdMap,
							GetSolidMap(coord),
							GetSolidMap(coord + VoxelConstants.NearCoordAdders[0]),
							GetSolidMap(coord + VoxelConstants.NearCoordAdders[1]),
							GetSolidMap(coord + VoxelConstants.NearCoordAdders[2]),
							GetSolidMap(coord + VoxelConstants.NearCoordAdders[3]),
							GetSolidMap(coord + VoxelConstants.NearCoordAdders[4]),
							GetSolidMap(coord + VoxelConstants.NearCoordAdders[5])
							);

						_waitBuildChunkCoords.RemoveAt(i--);

						_buildingChunkCoords.Add(coord);
					}
					else
					{
						_waitBuildChunkCoords.RemoveAt(i--);
					}
				}

				// Check Build Done
				for (int i = 0; i < _buildingChunkCoords.Count; i++)
				{
					var coord = _buildingChunkCoords[i];

					if (_chunks.TryGetValue(coord, out var chunk) && chunk.IsBuilding)
					{
						chunk.UpdateBuildMesh();

						if (!chunk.IsBuilding)
						{
							_buildingChunkCoords.RemoveAt(i--);
						}
					}
					else
					{
						// 모종의 이유로 청크 목록에서 사라졌다면 로그 출력
						Debug.LogError($"Building Chunk at { VoxelUtility.ConvertIdToPos(coord) } disappeared.");

						_buildingChunkCoords.RemoveAt(i--);
					}
				}

				// FIXME
				break;
			}
		}

		private bool[] GetSolidMap(int coord)
		{
			if (_chunkData.TryGetValue(coord, out var data))
			{
				return data.IsSolidMap;
			}

			return EmptySolidMap;
		}

		private void AddChunk(int coord)
		{
			if (_chunks.ContainsKey(coord))
			{
				Debug.LogError($"Cannot add Chunk at same coord : { VoxelUtility.ConvertIdToPos(coord) }");
				return;
			}

			if (!ServiceManager.TryGetService<IObjectPoolService>(out var objectPoolService))
			{
				return;
			}

			var spawnCoordPos = new Vector3(VoxelUtility.GetCoordX(coord), VoxelUtility.GetCoordY(coord), VoxelUtility.GetCoordZ(coord));
			var spawnPos = spawnCoordPos * VoxelConstants.ChunkAxisCount;

			var chunkGo = objectPoolService.Spawn(_chunkPoolGuid, spawnPos);
			var chunk = chunkGo.GetComponent<Chunk>();

			chunk.gameObject.name = $"Chunk ({VoxelUtility.GetCoordX(coord)}, {VoxelUtility.GetCoordY(coord)}, {VoxelUtility.GetCoordZ(coord)})";

			_chunks.Add(coord, chunk);

			chunk.Initialize();

			// FIXME : 테스트용 코드
			int chunkType;

			if (VoxelUtility.GetCoordY(coord) >= 0)
			{
				chunkType = -1;
			}
			else if (VoxelUtility.GetCoordY(coord) == -1)
			{
				chunkType = 1;
			}
			else
			{
				chunkType = 2;
			}

			if (!_chunkData.TryGetValue(coord, out var chunkData))
			{
				chunkData = new ChunkData(chunkType);

				_chunkData.Add(coord, chunkData);
			}

			if (chunkData.NeedBuild)
			{
				_waitBuildChunkCoords.Add(coord);
			}
		}

		private void RemoveChunk(int coord)
		{
			if (!_chunks.TryGetValue(coord, out var chunk))
			{
				Debug.LogError($"Has no Chunk to Remove at coord : { VoxelUtility.ConvertIdToPos(coord) }.");
				return;
			}

			chunk.Dispose();
			_chunks.Remove(coord);

			_waitBuildChunkCoords.Remove(coord);
			_buildingChunkCoords.Remove(coord);

			if (!ServiceManager.TryGetService<IObjectPoolService>(out var objectPoolService))
			{
				return;
			}

			objectPoolService.Despawn(chunk.gameObject);
		}
	}
}
