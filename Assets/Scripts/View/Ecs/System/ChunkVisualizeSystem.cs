using System.Collections.Generic;
using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Game.World.Stage;
using Service;
using Service.Rendering;
using UnityEngine;
using View.Ecs.Component;

namespace View.Ecs.System
{
	public class ChunkVisualizeSystem : ISystem
	{
		private Query<StagePropertyComponent> _stagePropertyQuery;

		private Query<ChunkComponent> _virtualChunkQuery;
		private Query<ChunkComponent, VisualizedChunkComponent> _visualizedChunkQuery;

		private Query<PlayerComponent, TransformComponent> _playerQuery;

		// FIXME : 이것도 Component로 빼자
		private int _currentCenterCoord = ChunkConstants.InvalidCoordId;

		private int _coordViewDistance = 0;

		private readonly List<Vector3Int> _visualizeLocalCoords = new();

		private readonly Dictionary<int, Entity> _virtualChunkBuffer = new();

		private readonly Dictionary<int, Entity> _visualizedChunkBuffer = new();

		public void Init(World world)
		{
			_stagePropertyQuery = new Query<StagePropertyComponent>(world);

			_playerQuery = new Query<PlayerComponent, TransformComponent>(world);

			_visualizedChunkQuery = new Query<ChunkComponent, VisualizedChunkComponent>(world);

			_virtualChunkQuery = new Query<ChunkComponent>(world);
			_virtualChunkQuery.Exclude<VisualizedChunkComponent>();
		}

		public void Update(float deltaTime)
		{
		}

		public void LateUpdate(float deltaTime)
		{
			if (!ServiceManager.TryGetService<IChunkService>(out var chunkService))
			{
				return;
			}

			foreach (var entity in _stagePropertyQuery)
			{
				ref var stagePropertyComponent = ref entity.Get<StagePropertyComponent>();

				if (stagePropertyComponent.CanGenerate)
				{
					return;
				}
			}

			chunkService.StartFetch();

			var viewDistChanged = false;

			// Distance가 플레이 도중 바뀌었을 때 캐싱해둔 CoordOffset을 갱신
			if (chunkService.CoordViewDistance != _coordViewDistance)
			{
				viewDistChanged = true;

				_coordViewDistance = chunkService.CoordViewDistance;

				_visualizeLocalCoords.Clear();

				for (int x = -_coordViewDistance; x <= _coordViewDistance; x++)
				{
					for (int y = -_coordViewDistance; y <= _coordViewDistance; y++)
					{
						for (int z = -_coordViewDistance; z <= _coordViewDistance; z++)
						{
							if (x * x + y * y + z * z <= _coordViewDistance * _coordViewDistance)
							{
								_visualizeLocalCoords.Add(new Vector3Int(x, y, z));
							}
						}
					}
				}

				_visualizeLocalCoords.Sort((a, b) => a.sqrMagnitude.CompareTo(b.sqrMagnitude));
			}

			foreach (var playerEntity in _playerQuery)
			{
				if (playerEntity.Has<NetworkEntityComponent>() &&
					playerEntity.Get<NetworkEntityComponent>().EntityRole == EntityRole.Remote)
				{
					continue;
				}

				var transformComponent = playerEntity.Get<TransformComponent>();
				var curPos = transformComponent.Position;
				var prevCenterCoord = _currentCenterCoord;

				_currentCenterCoord = ChunkUtility.GetCoordId(
					ChunkUtility.GetCoordAxis(curPos.x),
					ChunkUtility.GetCoordAxis(curPos.y),
					ChunkUtility.GetCoordAxis(curPos.z)
				);

				// 플레이어의 Coord나 시야 거리가 변경되었을 때
				if (_currentCenterCoord != prevCenterCoord || viewDistChanged)
				{
					// 시각화되어있던 청크들 중 범위에서 벗어나는 것은 가상화
					foreach (var visualizedChunkEntity in _visualizedChunkQuery)
					{
						var chunkComponent = visualizedChunkEntity.Get<ChunkComponent>();

						if (ChunkUtility.GetCoordSqrDistance(_currentCenterCoord, chunkComponent.coordId) >
						    _coordViewDistance * _coordViewDistance)
						{
							visualizedChunkEntity.Remove<VisualizedChunkComponent>();

							chunkService.RemoveVisualizer(visualizedChunkEntity);
						}
						else
						{
							// 시각화가 유지되어야 하는 것은 버퍼에 넣어둠
							_visualizedChunkBuffer.Add(chunkComponent.coordId, visualizedChunkEntity);
						}
					}

					// 가상 청크 버퍼 갱신
					foreach (var chunkEntity in _virtualChunkQuery)
					{
						var chunkComponent = chunkEntity.Get<ChunkComponent>();

						_virtualChunkBuffer.Add(chunkComponent.coordId, chunkEntity);
					}

					foreach (var localOffset in _visualizeLocalCoords)
					{
						// 활성화하려는 청크의 Coord가 존재한다면
						if (ChunkUtility.TryMoveCoord(_currentCenterCoord, localOffset.x, localOffset.y, localOffset.z,
							    out var movedCoordId) && _virtualChunkBuffer.TryGetValue(movedCoordId, out var chunkEntity))
						{
							chunkEntity.Add<VisualizedChunkComponent>();

							chunkService.AddVisualizer(chunkEntity);

							// 근처 6방향의 시각화된 청크도 갱신
							for (int i = 0; i < ChunkConstants.BlockSideCount; i++)
							{
								var nearOffset = ChunkConstants.NearVoxels[i];

								if (ChunkUtility.TryMoveCoord(movedCoordId,
									    nearOffset.x, nearOffset.y, nearOffset.z, out var nearCoordId))
								{
									if (_visualizedChunkBuffer.TryGetValue(nearCoordId, out var nearVisualizedChunkEntity))
									{
										chunkService.UpdateVisualizer(nearVisualizedChunkEntity);
									}
								}
							}
						}
					}

					_virtualChunkBuffer.Clear();
					_visualizedChunkBuffer.Clear();
				}

				// FIXME : Player가 여러 명이라면?
				break;
			}

			chunkService.EndFetch();
		}
	}
}
