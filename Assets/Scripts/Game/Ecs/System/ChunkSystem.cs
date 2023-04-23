using System;
using System.Collections.Generic;
using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Game.World;
using NUnit.Framework.Internal.Builders;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Ecs.System
{
	public class ChunkSystem : ISystem
	{
		private class RealizedArea
		{
			public int entityId;

			public BoundsInt bounds;

			public bool isUpdated;

			public RealizedArea(Entity entity, int chunkX, int chunkY)
			{
				this.entityId = entity.Id;

				bounds = new BoundsInt(
					chunkX - Constants.UpdatableChunkExtents, chunkY - Constants.UpdatableChunkExtents, 0,
					chunkX + Constants.UpdatableChunkExtents, chunkY + Constants.UpdatableChunkExtents, 0);

				this.isUpdated = true;
			}
		}

		private delegate void ChunkUpdater(int chunkX, int chunkY);

		private BlitzEcs.World _world;

		private Query<ChunkComponent> _chunkQuery;
		private Query<PlayerComponent, TransformComponent> _playerTransformQuery;

		private readonly List<RealizedArea> _realizedArea = new();

		private readonly Dictionary<int, int> _realizeReqCounts =
			new(Constants.UpdatableChunkExtents * Constants.UpdatableChunkExtents);
		private readonly Dictionary<int, int> _virtualizeReqCounts =
			new(Constants.UpdatableChunkExtents * Constants.UpdatableChunkExtents);

		/// <summary>
		/// FIXME : 이거 뭐로 설정해야 함...?
		/// </summary>
		Order ISystem.Order => Order.Highest;

		public void Init(BlitzEcs.World world)
		{
			_world = world;

			_chunkQuery = new(world);
			_playerTransformQuery = new(world);
		}

		public void Update(float deltaTime)
		{
		}

		public void LateUpdate(float deltaTime)
		{
			UpdateChunks();
		}

		private void UpdateChunks()
		{
			_chunkQuery.ForEach(((ref ChunkComponent chunkComponent) =>
			{
				// 이전 프레임에 저장해둔 Entity화된 영역들 업데이트 필요한 것으로 저장
				foreach (var area in _realizedArea)
				{
					area.isUpdated = false;
				}

				var totalChunks = chunkComponent.totalChunks;

				var totalUpdatableBounds = new BoundsInt(0, 0, 0,
					Constants.TotalChunkSize, Constants.TotalChunkSize, 0);

				// FIXME : 플레이어가 아닌 카메라를 기준으로 동작해야 함
				// Z축은 고려하지 않음.
				_playerTransformQuery.ForEach(((Entity entity, ref PlayerComponent _, ref TransformComponent transformComponent) =>
				{
					var position = transformComponent.Position;
					var xPos = position.x;
					var yPos = position.y;

					// 음수로 넘어가는 순간 청크 인덱스는 변경되어야 할 것이므로, 음수인 경우에만 -1을 해줌
					// Round 등을 실행하는 것이 16x16 청크에 크게 의미가 없으므로 int로 변환만 함
					int currentChunkX = (int)(xPos / Constants.ChunkUnitSize) + (xPos < 0 ? -1 : 0) + Constants.TotalChunkExtents;
					int currentChunkY = (int)(yPos / Constants.ChunkUnitSize) + (yPos < 0 ? -1 : 0) + Constants.TotalChunkExtents;

					RealizedArea prevUpdated = null;

					foreach (var updated in _realizedArea)
					{
						// 이전 프레임에서 들고있는 엔티티의 개수
						if (entity.Id == updated.entityId && updated.isUpdated)
						{
							prevUpdated = updated;
							break;
						}
					}

					// 처음 들어온 업데이트 요청인 경우
					if (prevUpdated == null)
					{
						var newBounds = new RealizedArea(entity, currentChunkX, currentChunkY);

						ForeachInBounds(newBounds.bounds, totalUpdatableBounds, RequestRealize);

						_realizedArea.Add(newBounds);
					}
					else
					{
						var curUpdatableBounds = new BoundsInt(
							currentChunkX - Constants.UpdatableChunkExtents, currentChunkY - Constants.UpdatableChunkExtents, 0,
							currentChunkX + Constants.UpdatableChunkExtents, currentChunkY + Constants.UpdatableChunkExtents, 0);

						if (math.abs(prevUpdated.bounds.x - curUpdatableBounds.x) < curUpdatableBounds.size.x &&
						    math.abs(prevUpdated.bounds.y - curUpdatableBounds.y) < curUpdatableBounds.size.y)
						{
							// 이전 바운드에 대해 멀어진 곳은 Virtualize하고,
							ExecuteChunkUpdate(prevUpdated.bounds, curUpdatableBounds,
								totalUpdatableBounds, RequestVirtualize);

							// 새로 가까워진 곳은 Realize
							ExecuteChunkUpdate(curUpdatableBounds, prevUpdated.bounds,
								totalUpdatableBounds, RequestRealize);
						}
						else
						{
							// (많이는 없겠지만) 한 프레임만에 청크 8칸을 건너뛴 경우
							// 이전 영역은 모두 비활성화하고
							ForeachInBounds(prevUpdated.bounds, totalUpdatableBounds, RequestVirtualize);

							// 현재 영역은 모두 활성화
							ForeachInBounds(curUpdatableBounds, totalUpdatableBounds, RequestRealize);
						}

						prevUpdated.bounds = curUpdatableBounds;
						prevUpdated.isUpdated = true;
					}
				}));

				for (int i = 0; i < _realizedArea.Count; i++)
				{
					if (_realizedArea[i].isUpdated)
					{
						continue;
					}

					// 이번 프레임에 한 번도 업데이트되지 않은 곳은 비활성화해줌
					ForeachInBounds(_realizedArea[i].bounds, totalUpdatableBounds, RequestVirtualize);

					_realizedArea.RemoveAt(i--);
				}

				// Entity화 및 Entity 비활성화 요청을 나누어 관리.
				// 풀링되는 전체 인스턴스들이 최소가 되도록 하기 위해 Virtualize를 먼저 처리

				// Virtualize될 청크들 처리
				foreach (var keyValue in _virtualizeReqCounts)
				{
					// FIXME : 원래는 시프트 연산으로 퍼포먼스를 올려야 함
					var chunkX = keyValue.Key / Constants.TotalChunkSize;
					var chunkY = keyValue.Key % Constants.TotalChunkSize;

					totalChunks[chunkX, chunkY].IsRealized = false;

					// TODO : Entity 데이터화
					// for (int i = 0; i < Constants.ChunkUnitSize; i++)
					// {
					// 	for (int j = 0; j < Constants.ChunkUnitSize; j++)
					// 	{
					// 		var entityId = totalChunks[chunkX, chunkY].cells[i, j].entityId;
					//
					// 		if (entityId != null)
					// 		{
					// 			_world.Despawn(entityId.Value);
					//
					// 			totalChunks[chunkX, chunkY].cells[i, j].entityId = null;
					// 		}
					// 		else
					// 		{
					// 			// 있어서는 안되는 일. 체크 필요
					// 		}
					// 	}
					// }
				}

				_virtualizeReqCounts.Clear();

				// Realize될 청크들 처리
				foreach (var keyValue in _realizeReqCounts)
				{
					// FIXME : 원래는 시프트 연산으로 퍼포먼스를 올려야 함
					var chunkX = keyValue.Key / Constants.TotalChunkSize;
					var chunkY = keyValue.Key % Constants.TotalChunkSize;

					totalChunks[chunkX, chunkY].IsRealized = true;

					// TODO : Unit Entity 실제 GameObject로 변환
					// for (int i = 0; i < Constants.ChunkUnitSize; i++)
					// {
					// 	for (int j = 0; j < Constants.ChunkUnitSize; j++)
					// 	{
					// 		var newEntity = _world.Spawn();
					// 		totalChunks[chunkX, chunkY].cells[i, j].entityId = newEntity.Id;
					// 	}
					// }
				}

				_realizeReqCounts.Clear();
			}));
		}

		/// <summary>
		/// bounds 내부 모든 정수형 좌표에 대해 특정 콜백을 처리
		/// </summary>
		private void ForeachInBounds(BoundsInt bounds, BoundsInt clampBounds, ChunkUpdater updater)
		{
			bounds.ClampToBounds(clampBounds);

			for (int x = bounds.xMin; x < bounds.xMax; x++)
			{
				for (int y = bounds.yMin; y < bounds.yMax; y++)
				{
					updater(x, y);
				}
			}
		}

		/// <summary>
		/// 청크 x, y 인덱스에 대해 Id를 생성해서 넘겨줌
		/// </summary>
		private int GetChunkId(int x, int y)
		{
			// FIXME : 원래는 시프트 연산으로 퍼포먼스를 올려야 함
			return x * Constants.TotalChunkSize + y;
		}

		/// <summary>
		/// 한 청크에 대해 Realize(Entity화) 요청
		/// </summary>
		private void RequestRealize(int x, int y)
		{
			var id = GetChunkId(x, y);

			if (_virtualizeReqCounts.ContainsKey(id))
			{
				if (--_virtualizeReqCounts[id] == 0)
				{
					_virtualizeReqCounts.Remove(id);
				}

				return;
			}

			if (!_realizeReqCounts.ContainsKey(id))
			{
				_realizeReqCounts.Add(id, 0);
			}

			_realizeReqCounts[id]++;
		}

		/// <summary>
		/// 한 청크에 대해 Virtualize(Entity 해제) 요청
		/// </summary>
		private void RequestVirtualize(int x, int y)
		{
			var id = GetChunkId(x, y);

			if (_realizeReqCounts.ContainsKey(id))
			{
				if (--_realizeReqCounts[id] == 0)
				{
					_realizeReqCounts.Remove(id);
				}

				return;
			}

			if (!_realizeReqCounts.ContainsKey(id))
			{
				_realizeReqCounts.Add(id, 0);
			}

			_realizeReqCounts[id]--;
		}

		/// <summary>
		/// a 기준 활성화 바운드에서 b 기준 활성화 바운드를 뺀 나머지 영역에 대해 updater 실행
		/// </summary>
		private void ExecuteChunkUpdate(BoundsInt aBounds, BoundsInt bBounds, BoundsInt clampBounds, ChunkUpdater updater)
		{
			// 좌우 계산. ㄱ자 모양에서 오른쪽 부분
			{
				var startX = 0;
				var endX = 0;
				var startY = math.max(aBounds.min.y, bBounds.min.y);
				var endY = math.min(aBounds.max.y, bBounds.max.y);
				var needCalc = false;

				if (aBounds.position.x < bBounds.position.x)
				{
					startX = aBounds.xMax;
					endX = bBounds.xMax;

					needCalc = true;
				}
				else if (aBounds.position.x > bBounds.position.x)
				{
					startX = aBounds.xMin;
					endX = bBounds.xMin;

					needCalc = true;
				}

				if (needCalc)
				{
					var targetBounds = new BoundsInt(startX, startY, 0, endX, endY, 0);

					ForeachInBounds(targetBounds, clampBounds, updater);
				}
			}

			// 상하 계산. ㄱ자 모양에서 윗 부분
			{
				var startX = 0;
				var endX = 0;
				var startY = 0;
				var endY = 0;
				var needCalc = false;

				if (aBounds.position.y < bBounds.position.y)
				{
					startY = aBounds.yMax;
					endY = bBounds.yMax;

					needCalc = true;
				}
				else if (aBounds.position.y > bBounds.position.y)
				{
					startY = aBounds.yMin;
					endY = bBounds.yMin;

					needCalc = true;
				}

				if (needCalc)
				{
					if (aBounds.position.x < bBounds.position.x)
					{
						startX = bBounds.xMin;
						endX = bBounds.xMax;
					}
					else
					{
						startX = aBounds.xMin;
						endX = aBounds.xMax;
					}

					var targetBounds = new BoundsInt(startX, startY, 0, endX, endY, 0);

					ForeachInBounds(targetBounds, clampBounds, updater);
				}
			}
		}
	}
}
