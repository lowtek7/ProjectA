using System;
using BlitzEcs;
using Core.Unity;
using Core.Utility;
using Game.Ecs.Component;
using Game.Extensions;
using RAMG.Packets;
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
				ref var movementComponent = ref entity.Get<MovementComponent>();

				// MoveDir가 zero가 아니라면 계산해줌
				if (movementComponent.MoveDir != Vector3.zero)
				{
					ServiceManager.TryGetService<ICollisionService>(out var collisionService);

					var dir = movementComponent.MoveDir;
					var dist = movementComponent.CurrentSpeed * deltaTime;

					if (entity.IsLocalEntity())
					{
						if (collisionService.IsCollision(entity, (dir * dist)))
						{
							// 충돌 알림
						}
						else
						{
							var velocity = dir * movementComponent.CurrentSpeed;
							var netIdComponent = entity.Get<NetworkEntityComponent>();

							if (ServiceManager.TryGetService(out INetClientService clientService))
							{
								var pos = transformComponent.Position;
								// dispose는 받는쪽에서 알아서 해줄 예정.
								var command = CMD_ENTITY_MOVE.Create();

								command.Id = netIdComponent.NetId;
								command.Time = DateTime.UtcNow.ToUnixTime();
								command.MoveType = MoveType.None;
								command.X = pos.x;
								command.Y = pos.y;
								command.Z = pos.z;
								command.VelocityX = velocity.x;
								command.VelocityY = velocity.y;
								command.VelocityZ = velocity.z;

								if (movementComponent.IsMoving)
								{
									command.MoveType = MoveType.Walk;
								}

								if (movementComponent.IsRunning)
								{
									command.MoveType = MoveType.Run;
								}

								clientService.SendCommand(command);
							}

							transformComponent.Position += velocity * deltaTime;
						}
					}
				}
				else
				{
					// 캐릭터가 움직이지 않는다면 뭉쳐서 보내는 동기화 패킷에 같이 보내도록 설정.
					if (entity.IsLocalEntity())
					{
						var netIdComponent = entity.Get<NetworkEntityComponent>();

						if (ServiceManager.TryGetService(out INetClientService clientService))
						{
							var pos = transformComponent.Position;
							// dispose는 받는쪽에서 알아서 해줄 예정.
							var command = CMD_ENTITY_POS_SYNC.Create();
							var key = $"MOVE_IDLE_{netIdComponent.NetId}";

							command.Id = netIdComponent.NetId;
							command.Time = DateTime.UtcNow.ToUnixTime();
							command.X = pos.x;
							command.Y = pos.y;
							command.Z = pos.z;

							clientService.SendPeriodCommand(key, PeriodPacketType.HalfSecond, command);
						}
					}
				}

				// 캐릭터 회전
				if (entity.IsLocalEntity() && !transformComponent.Rotation.eulerAngles.y.IsAlmostCloseTo(movementComponent.TargetRotation.eulerAngles.y))
				{
					var rotationDist = movementComponent.RotateSpeed * deltaTime;
					var currentRotation = Quaternion.RotateTowards(transformComponent.Rotation,
						movementComponent.TargetRotation, rotationDist);

					transformComponent.Rotation = currentRotation;

					var playerComponent = entity.Get<PlayerComponent>();
					var netIdComponent = entity.Get<NetworkEntityComponent>();

					if (ServiceManager.TryGetService(out INetClientService clientService))
					{
						// dispose는 받는쪽에서 알아서 해줄 예정.
						var command = CMD_ENTITY_ROTATE.Create();

						command.Id = netIdComponent.NetId;
						command.Time = DateTime.UtcNow.ToUnixTime();
						command.X = currentRotation.x;
						command.Y = currentRotation.y;
						command.Z = currentRotation.z;
						command.W = currentRotation.w;

						clientService.SendCommand(command);
					}
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
