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

				if (!netMovementComponent.IsMoving)
				{
					movementComponent.IsRun = false;
					movementComponent.MoveDir = Vector3.zero;

					continue;
				}

				movementComponent.IsRun = netMovementComponent.IsRun;
				transformComponent.Position += velocity * deltaTime;
				movementComponent.MoveDir = velocity.normalized;

				if (netMovementComponent.TargetPos.HasValue)
				{
					var targetPos = netMovementComponent.TargetPos.Value;

					if (targetPos.IsAlmostCloseTo(transformComponent.Position))
					{
						netMovementComponent.IsMoving = false;
						movementComponent.IsRun = netMovementComponent.IsRun;
						movementComponent.MoveDir = velocity.normalized;
						netMovementComponent.TargetPos = null;
					}
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
