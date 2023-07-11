using System;
using System.Collections.Generic;
using System.Linq;
using BlitzEcs;
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
		private int loadCoordMagnitude = 3;

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

		private static readonly bool[] EmptySolidMap = new bool[
			VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount
		];

		private const int MaxMeshBuildingJobCount = 5;

		private class ChunkData
		{
			public bool[] IsSolidMap;
			public int[] BlockIdMap;
			// FIXME : 임시
			public int ChunkType;

			public ChunkData(int chunkType)
			{
				ChunkType = chunkType;

				BlockIdMap = new int[VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount];
				IsSolidMap = new bool[VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount];

				for (int y = 0; y < VoxelConstants.ChunkAxisCount; y++)
				{
					var yWeight = y << VoxelConstants.ChunkAxisExponent;

					for (int x = 0; x < VoxelConstants.ChunkAxisCount; x++)
					{
						var xWeight = x << (VoxelConstants.ChunkAxisExponent << 1);

						for (int z = 0; z < VoxelConstants.ChunkAxisCount; z++)
						{
							var index = xWeight | yWeight | z;

							if (ChunkType == 1 && y < VoxelConstants.ChunkAxisCount - 1)
							{
								BlockIdMap[index] = 2;
							}
							else
							{
								BlockIdMap[index] = ChunkType;
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
			for (int x = -loadCoordMagnitude; x <= loadCoordMagnitude; x++)
			{
				for (int y = -loadCoordMagnitude; y <= loadCoordMagnitude; y++)
				{
					for (int z = -loadCoordMagnitude; z <= loadCoordMagnitude; z++)
					{
						var coordVector = new Vector3Int(x, y, z);

						if (coordVector.sqrMagnitude <= loadCoordMagnitude * loadCoordMagnitude)
						{
							_chunkLocalCoords.Add(coordVector);
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
					foreach (var keyValue in _chunks)
					{
						var coord = keyValue.Key;

						if (VoxelUtility.GetCoordSqrDistance(_currentCenterCoord, coord) > loadCoordMagnitude * loadCoordMagnitude)
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
						// 해당 좌표가 청크 범위를 넘어서지 않을 때만 체크
						if (VoxelUtility.TryMoveCoord(_currentCenterCoord, localOffset.x, localOffset.y, localOffset.z, out var movedCoordId))
						{
							// Visualize되어있지 않은 경우만 체크
							if (!_chunks.ContainsKey(movedCoordId))
							{
								AddChunk(movedCoordId);
							}
						}
					}
				}

				// Streaming
				for (int i = 0; i < _waitBuildChunkCoords.Count; i++)
				{
					var coord = _waitBuildChunkCoords[i];

					if (_chunks.TryGetValue(coord, out var chunk))
					{
						if (chunk.State == ChunkState.Activated && _buildingChunkCoords.Count < MaxMeshBuildingJobCount)
						{
							var blockIdMap = _chunkData[coord].BlockIdMap;

							chunk.RebuildMesh(
								blockIdMap,
								GetSolidMap(coord),
								GetSolidMap(coord - (1 << VoxelConstants.ChunkCoordXExponent)),
								GetSolidMap(coord + (1 << VoxelConstants.ChunkCoordXExponent)),
								GetSolidMap(coord + (1 << VoxelConstants.ChunkCoordYExponent)),
								GetSolidMap(coord - (1 << VoxelConstants.ChunkCoordYExponent)),
								GetSolidMap(coord + (1 << VoxelConstants.ChunkCoordZExponent)),
								GetSolidMap(coord - (1 << VoxelConstants.ChunkCoordZExponent))
								);

							_waitBuildChunkCoords.RemoveAt(i--);

							_buildingChunkCoords.Add(coord);
						}
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

					if (_chunks.TryGetValue(coord, out var chunk))
					{
						chunk.UpdateBuildMesh();

						if (chunk.State == ChunkState.Done)
						{
							_buildingChunkCoords.RemoveAt(i--);
						}
					}
					else
					{
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
				Debug.LogError("왠진 모르겠는데 청크 인덱스 좌표가 이미 들어가있음");
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

			if (!_chunkData.ContainsKey(coord))
			{
				var chunkData = new ChunkData(chunkType);

				_chunkData.Add(coord, chunkData);
			}

			if (chunkType != -1)
			{
				_waitBuildChunkCoords.Add(coord);
			}
		}

		private void RemoveChunk(int coord)
		{
			if (!_chunks.TryGetValue(coord, out var chunk))
			{
				Debug.LogError("왠진 모르겠는데 청크 인덱스 좌표가 없음");
				return;
			}

			_chunks.Remove(coord);

			if (!ServiceManager.TryGetService<IObjectPoolService>(out var objectPoolService))
			{
				return;
			}

			objectPoolService.Despawn(chunk.gameObject);

			_waitBuildChunkCoords.Remove(coord);
			_buildingChunkCoords.Remove(coord);
		}
	}
}
