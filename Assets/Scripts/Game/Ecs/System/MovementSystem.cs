﻿using System;
using BlitzEcs;
using Core.Unity;
using Core.Utility;
using Game.Ecs.Component;
using Network.NetCommand.Client.Entity;
using Service;
using Service.Collision;
using Service.Network;
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
					ServiceManager.TryGetService<ICollisionService>(out var collisionService);

					var dir = movementComponent.MoveDir;
					var dist = movementComponent.CurrentSpeed * deltaTime;

					if (collisionService.IsCollision(entity, (dir * dist)))
					{
						// 충돌 알림
					}
					else
					{
						transformComponent.Position += (dir * dist);
					}

					if (entity.Has<PlayerComponent>() && entity.Has<NetIdComponent>())
					{
						var playerComponent = entity.Get<PlayerComponent>();
						var netIdComponent = entity.Get<NetIdComponent>();

						if (playerComponent.PlayerType == PlayerType.Local)
						{
							if (ServiceManager.TryGetService(out INetClientService clientService))
							{
								var pos = transformComponent.Position;
								// dispose는 받는쪽에서 알아서 해줄 예정.
								var command = CMD_ENTITY_MOVE.Create();

								command.Id = netIdComponent.NetId;
								command.Time = DateTime.UtcNow.ToUnixTime();
								command.MovementFlags = MovementFlags.None;
								command.SetPosition(pos.x, pos.y, pos.z);

								if (movementComponent.IsMoving)
								{
									command.MovementFlags |= MovementFlags.Walk;
								}

								if (movementComponent.IsRunning)
								{
									command.MovementFlags |= MovementFlags.Run;
								}

								clientService.SendCommand(command);
							}
						}
					}
				}

				// 캐릭터 회전
				if (!transformComponent.Rotation.eulerAngles.y.IsAlmostCloseTo(movementComponent.TargetRotation.eulerAngles.y))
				{
					var rotationDist = movementComponent.RotateSpeed * deltaTime;
					var currentRotation = Quaternion.RotateTowards(transformComponent.Rotation,
						movementComponent.TargetRotation, rotationDist);

					transformComponent.Rotation = currentRotation;

					if (entity.Has<PlayerComponent>() && entity.Has<NetIdComponent>())
					{
						var playerComponent = entity.Get<PlayerComponent>();
						var netIdComponent = entity.Get<NetIdComponent>();

						if (playerComponent.PlayerType == PlayerType.Local)
						{
							if (ServiceManager.TryGetService(out INetClientService clientService))
							{
								// dispose는 받는쪽에서 알아서 해줄 예정.
								var command = CMD_ENTITY_MOVE.Create();

								command.Id = netIdComponent.NetId;
								command.Time = DateTime.UtcNow.ToUnixTime();
								command.MovementFlags = MovementFlags.Rotate;
								command.SetRotation(currentRotation.x, currentRotation.y, currentRotation.z, currentRotation.w);

								clientService.SendCommand(command);
							}
						}
					}
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
