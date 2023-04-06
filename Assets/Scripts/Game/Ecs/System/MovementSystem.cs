﻿using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using UnityEngine;

namespace Game.Ecs.System
{
	public class MovementSystem : ISystem
	{
		private Query<TransformComponent, MovementComponent> moveQuery;

		public int Order => 1;

		public void Init(BlitzEcs.World world)
		{
			moveQuery = new Query<TransformComponent, MovementComponent>(world);
		}

		public void Update(float deltaTime)
		{
			moveQuery.Fetch();
			moveQuery.ForEach((Entity entity,
				ref TransformComponent transformComponent,
				ref MovementComponent movementComponent) =>
			{
				// MoveDir가 zero가 아니라면 계산해줌
				if (movementComponent.MoveDir != Vector3.zero)
				{
					// 여기서 normalize하는것이 과연 올바른것일까?
					var dir = movementComponent.MoveDir.normalized;
					var speed = movementComponent.MoveSpeed * deltaTime;
					transformComponent.Position += (dir * speed);
				}
			});
		}
	}
}