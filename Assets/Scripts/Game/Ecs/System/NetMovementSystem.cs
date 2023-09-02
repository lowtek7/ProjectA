using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Game.Extensions;
using UnityEngine;

namespace Game.Ecs.System
{
	public class NetMovementSystem : ISystem
	{
		private Query<TransformComponent, MovementComponent, NetMovementComponent> _moveQuery;

		private BlitzEcs.World _world;

		public void Init(BlitzEcs.World world)
		{
			_moveQuery = new Query<TransformComponent, MovementComponent, NetMovementComponent>(world);
			_world = world;
		}

		public void Update(float deltaTime)
		{
			foreach (var entity in _moveQuery)
			{
				if (entity.IsLocalEntity())
				{
					// 잘못된 객체임. 로컬 엔티티가 NetMovementComponent를 가지고 있으면 안된다.
					continue;
				}

				ref var transformComponent = ref entity.Get<TransformComponent>();
				ref var movementComponent = ref entity.Get<MovementComponent>();
				ref var netMovementComponent = ref entity.Get<NetMovementComponent>();

				if (!netMovementComponent.IsMoving)
				{
					transformComponent.Position = netMovementComponent.GoalPos;
					continue;
				}

				var targetPos = Vector3.Lerp(transformComponent.Position, netMovementComponent.GoalPos, deltaTime);
				var speed = (targetPos - transformComponent.Position).magnitude;

				if (speed >= movementComponent.RunSpeed * deltaTime)
				{
					movementComponent.IsRun = true;
				}
				else
				{
					movementComponent.IsRun = false;
				}

				transformComponent.Position = targetPos;
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
