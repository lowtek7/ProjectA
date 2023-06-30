using System;
using BlitzEcs;
using Core.Unity;
using Core.Utility;
using Game.Ecs.Component;
using UnityEngine;

namespace Game.Ecs.System
{
	public class MovementSystem : ISystem
	{
		private Query<TransformComponent, MovementComponent> _moveQuery;

		private BlitzEcs.World _world;

		public void Init(BlitzEcs.World world)
		{
			_moveQuery = new Query<TransformComponent, MovementComponent>(world);
			_world = world;
		}

		public void Update(float deltaTime)
		{
			foreach (var entity in _moveQuery)
			{
				ref var transformComponent = ref entity.Get<TransformComponent>();
				var movementComponent = entity.Get<MovementComponent>();

				// MoveDir가 zero가 아니라면 계산해줌
				if (movementComponent.MoveDir != Vector3.zero)
				{
					// 여기서 normalize하는것이 과연 올바른것일까?
					var dir = movementComponent.MoveDir;
					var dist = movementComponent.CurrentSpeed * deltaTime;

					transformComponent.Position += (dir * dist);
				}

				// 캐릭터 회전
				if (!transformComponent.Rotation.eulerAngles.y.IsAlmostCloseTo(movementComponent.TargetRotation.eulerAngles.y))
				{
					var rotationDist = movementComponent.RotateSpeed * deltaTime;
					var currentRotation = Quaternion.RotateTowards(transformComponent.Rotation,
						movementComponent.TargetRotation, rotationDist);

					transformComponent.Rotation = currentRotation;
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
