using System;
using System.Collections.Generic;
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

		private Guid _chunkPoolGuid;

		private World _world;
		private Query<PlayerComponent, TransformComponent> _playerQuery;

		private readonly Dictionary<Vector3Int, IChunk> _chunks = new();

		private List<Vector3Int> _removeChunkCoordBuffer;
		private List<Vector3Int> _addChunkCoordBuffer;

		private List<Vector3Int> _waitBuildChunkCoords;

		[SerializeField]
		private int loadCoordMagnitude = 3;
		private List<Vector3Int> _chunkLocalCoords = new();

		private Vector3Int _currentCenterCoord = Vector3Int.zero;

		private Dictionary<string, PackedTextureUvInfo[]> _uvInfos;

		public static readonly Dictionary<int, PackedTextureUvInfo> UvInfo = new();

		private static readonly bool[] EmptySolidMap = new bool[
			VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount
		];

		private readonly List<Vector3Int> _buildingChunkCoords = new();

		private const int MaxMeshBuildingJobCount = 5;

		public void Init(World world)
		{
			_world = world;

			_playerQuery = new Query<PlayerComponent, TransformComponent>(world);

			// 완전 구석탱이로 보내버린 다음에 청크를 새로 생성하도록 함
			// _currentCenterCoord = Vector3Int.one * (Int32.MaxValue - _loadCoordMagnitude);

			// 대략 구체에 가까운 모양새로 생성
			for (int x = -loadCoordMagnitude; x <= loadCoordMagnitude; x++)
			{
				for (int y = -loadCoordMagnitude; y <= loadCoordMagnitude; y++)
				{
					for (int z = -loadCoordMagnitude; z <= loadCoordMagnitude; z++)
					{
						var coord = new Vector3Int(x, y, z);

						if (coord.sqrMagnitude <= loadCoordMagnitude * loadCoordMagnitude)
						{
							_chunkLocalCoords.Add(coord);
						}
					}
				}
			}

			_chunkLocalCoords.Sort((a, b) => a.sqrMagnitude.CompareTo(b.sqrMagnitude));

			// 동시에 사용 가능한 Coord 개수로 Add/Remove 버퍼 사이즈를 잡아줌
			var bufferSize = _chunkLocalCoords.Count;

			_removeChunkCoordBuffer = new(bufferSize);
			_addChunkCoordBuffer = new(bufferSize);
			_waitBuildChunkCoords = new(bufferSize);

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

				_currentCenterCoord = new Vector3Int(
					GetChunkIndex(curPos.x),
					GetChunkIndex(curPos.y),
					GetChunkIndex(curPos.z)
					);

				// Remove
				foreach (var keyValue in _chunks)
				{
					var coord = keyValue.Key;

					if ((_currentCenterCoord - coord).sqrMagnitude > loadCoordMagnitude * loadCoordMagnitude)
					{
						var nextChunkCoord = prevCenterCoord - coord + _currentCenterCoord;

						_addChunkCoordBuffer.Add(nextChunkCoord);

						_removeChunkCoordBuffer.Add(coord);
					}
				}

				foreach (var coord in _removeChunkCoordBuffer)
				{
					RemoveChunk(coord);
				}

				_removeChunkCoordBuffer.Clear();

				// Add
				if (_chunks.Count == 0)
				{
					foreach (var localCoord in _chunkLocalCoords)
					{
						var coord = _currentCenterCoord + localCoord;

						AddChunk(coord);
					}

					_chunkLocalCoords = null;
				}
				else
				{
					foreach (var coord in _addChunkCoordBuffer)
					{
						AddChunk(coord);
					}
				}

				_addChunkCoordBuffer.Clear();

				// Streaming
				for (int i = 0; i < _waitBuildChunkCoords.Count; i++)
				{
					var coord = _waitBuildChunkCoords[i];

					if (_chunks.TryGetValue(coord, out var chunk))
					{
						if (chunk.State == ChunkState.WaitBuild && _buildingChunkCoords.Count < MaxMeshBuildingJobCount)
						{
							chunk.RebuildMesh(
								GetSolidMap(coord + Vector3Int.left),
								GetSolidMap(coord + Vector3Int.right),
								GetSolidMap(coord + Vector3Int.up),
								GetSolidMap(coord + Vector3Int.down),
								GetSolidMap(coord + Vector3Int.forward),
								GetSolidMap(coord + Vector3Int.back)
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

		private bool[] GetSolidMap(Vector3Int coord)
		{
			if (_chunks.TryGetValue(coord, out var chunk))
			{
				return chunk.IsSolidMap;
			}

			return EmptySolidMap;
		}

		private int GetChunkIndex(float value)
		{
			return Mathf.RoundToInt(value) >> VoxelConstants.ChunkAxisCount;
		}

		private void AddChunk(Vector3Int coord)
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

			var newChunk = objectPoolService.Spawn<Chunk>(
				_chunkPoolGuid, coord * VoxelConstants.ChunkAxisCount);

			_chunks.Add(coord, newChunk);

			newChunk.Initialize(coord);

			if (newChunk.State == ChunkState.WaitBuild)
			{
				_waitBuildChunkCoords.Add(coord);
			}
		}

		private void RemoveChunk(Vector3Int coord)
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

			objectPoolService.Despawn(chunk.GameObject);

			_waitBuildChunkCoords.Remove(coord);
			_buildingChunkCoords.Remove(coord);
		}
	}
}
