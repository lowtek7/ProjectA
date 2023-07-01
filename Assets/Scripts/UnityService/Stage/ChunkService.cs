using System;
using System.Collections.Generic;
using BlitzEcs;
using Game.Ecs.Component;
using Service;
using Service.ObjectPool;
using Service.Stage;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityService.Stage
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

		private readonly Dictionary<Vector3Int, Chunk> _chunks = new();

		private List<Vector3Int> _removeChunkCoordBuffer;
		private List<Vector3Int> _addChunkCoordBuffer;
		private List<Vector3Int> _addedChunkCoordBuffer;

		[SerializeField]
		private int loadCoordMagnitude = 3;
		private List<Vector3Int> _chunkLocalCoords = new();

		private Vector3Int _currentCenterCoord = Vector3Int.zero;

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

			// 동시에 사용 가능한 Coord 개수로 Add/Remove 버퍼 사이즈를 잡아줌
			var bufferSize = _chunkLocalCoords.Count;

			_removeChunkCoordBuffer = new(bufferSize);
			_addChunkCoordBuffer = new(bufferSize);
			_addedChunkCoordBuffer = new(bufferSize);

			_chunkPoolGuid = new Guid(chunkPoolGuid);
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

				if (_removeChunkCoordBuffer.Count > 0)
				{
					Debug.LogError(_removeChunkCoordBuffer.Count);
				}

				_removeChunkCoordBuffer.Clear();

				// Add
				// FIXME : 제일 첫 생성을 한 방에 처리할 방법이 없을까?
				// Preset 메소드를 하나 만들까 했는데, 그러면 결국 업데이트 어딘가에서 해당 조건을 체크하고 있어야 해서 똑같은 상황이라 이대로 둠
				if (_chunks.Count == 0)
				{
					foreach (var localCoord in _chunkLocalCoords)
					{
						var coord = _currentCenterCoord + localCoord;

						if (AddChunk(coord))
						{
							_addedChunkCoordBuffer.Add(coord);
						}
					}

					_chunkLocalCoords = null;
				}
				else
				{
					foreach (var coord in _addChunkCoordBuffer)
					{
						if (AddChunk(coord))
						{
							_addedChunkCoordBuffer.Add(coord);
						}
					}
				}

				_addChunkCoordBuffer.Clear();

				foreach (var coord in _addedChunkCoordBuffer)
				{
					if (_chunks.TryGetValue(coord, out var chunk))
					{
						chunk.RebuildMesh();
					}
				}

				_addedChunkCoordBuffer.Clear();

				// FIXME
				break;
			}
		}

		private int GetChunkIndex(float value)
		{
			return Mathf.RoundToInt(value) >> VoxelConstants.ChunkAxisCount;
		}

		private bool AddChunk(Vector3Int coord)
		{
			if (_chunks.ContainsKey(coord))
			{
				Debug.LogError("왠진 모르겠는데 청크 인덱스 좌표가 이미 들어가있음");
				return false;
			}

			if (!ServiceManager.TryGetService<IObjectPoolService>(out var objectPoolService))
			{
				return false;
			}

			var newChunk = objectPoolService.Spawn<Chunk>(
				_chunkPoolGuid, coord * VoxelConstants.ChunkAxisCount);

			_chunks.Add(coord, newChunk);

			newChunk.Initialize(coord);

			return true;
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

			objectPoolService.Despawn(chunk.gameObject);
		}

		public bool IsSolidAtOuter(Vector3Int chunkCoord, Vector3Int localPos)
		{
			var nearDir = Vector3Int.zero;

			if (localPos.x < 0)
			{
				nearDir = Vector3Int.left;
			}
			else if (localPos.x >= VoxelConstants.ChunkAxisCount)
			{
				nearDir = Vector3Int.right;
			}
			else if (localPos.y < 0)
			{
				nearDir = Vector3Int.down;
			}
			else if (localPos.y >= VoxelConstants.ChunkAxisCount)
			{
				nearDir = Vector3Int.up;
			}
			else if (localPos.z < 0)
			{
				nearDir = Vector3Int.back;
			}
			else if (localPos.z >= VoxelConstants.ChunkAxisCount)
			{
				nearDir = Vector3Int.forward;
			}

			if (nearDir == Vector3Int.zero)
			{
				return false;
			}

			var otherCoord = chunkCoord + nearDir;

			if (!_chunks.TryGetValue(otherCoord, out var chunk))
			{
				return false;
			}

			var otherPos = localPos - nearDir * VoxelConstants.ChunkAxisCount;

			return chunk.IsSolidAt(otherPos);
		}
	}
}
