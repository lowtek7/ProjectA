using BlitzEcs;
using Core.Unity;
using Core.Utility;
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

				var velocity = netMovementComponent.Velocity;
				var dir = netMovementComponent.Velocity.normalized;
				var speed = netMovementComponent.Velocity.magnitude;

				if (!netMovementComponent.IsMoving)
				{
					if (!netMovementComponent.GoalPos.IsAlmostCloseTo(transformComponent.Position) &&
						netMovementComponent.Velocity != Vector3.zero)
					{
						movementComponent.IsRun = netMovementComponent.IsRun;
						transformComponent.Position += dir * (speed * deltaTime);
						movementComponent.MoveDir = dir;
					}
					else
					{
						movementComponent.IsRun = false;
						movementComponent.MoveDir = Vector3.zero;
					}

					continue;
				}

				movementComponent.IsRun = netMovementComponent.IsRun;
				transformComponent.Position += dir * (speed * deltaTime);
				movementComponent.MoveDir = dir;

				if (netMovementComponent.GoalPos.IsAlmostCloseTo(transformComponent.Position))
				{
					netMovementComponent.IsMoving = false;
					movementComponent.MoveDir = Vector3.zero;
					movementComponent.IsRun = false;
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
