using System;
using BlitzEcs;
using Core.Unity;
using Core.Utility;
using Game.Ecs.Component;
using Game.Extensions;
using RAMG.Packets;
using Service;
using Service.Camera;
using Service.Input;
using Service.Network;
using UnityEngine;

namespace Game.Ecs.System
{
	public class InputSystem : ISystem
	{
		private Query<InputComponent> _inputQuery;

		private Query<MovementComponent, TransformComponent, PlayerComponent> _movementQuery;

		public void Init(BlitzEcs.World world)
		{
			_inputQuery = new (world);
			_movementQuery = new (world);
		}

		public void Update(float deltaTime)
		{
			if (ServiceManager.TryGetService<IGameInputService>(out var inputService))
			{
				inputService.Fetch();
			}

			foreach (var entity in _inputQuery)
			{
				ref var inputComponent = ref entity.Get<InputComponent>();
				var moveDirection = inputComponent.MoveDirection;

				foreach (var moveEntity in _movementQuery)
				{
					if (moveEntity.IsRemoteEntity())
					{
						continue;
					}

					ref var movementComponent = ref moveEntity.Get<MovementComponent>();
					ref var transformComponent = ref moveEntity.Get<TransformComponent>();

					// 임시적으로 디렉션을 무브 디렉션을 사용하게 하자.
					if (moveDirection != Vector3.zero)
					{
						transformComponent.Direction = moveDirection;
						movementComponent.TargetRotation = Quaternion.LookRotation(moveDirection);
					}
					else
					{
						// 입력이 없고 이전 프레임에 움직였었다면 처리해주자.
						if (movementComponent.MoveDir != Vector3.zero)
						{
							if (ServiceManager.TryGetService(out INetClientService clientService) && entity.Has<NetworkEntityComponent>())
							{
								var netIdComponent = entity.Get<NetworkEntityComponent>();
								var pos = transformComponent.Position;
								// dispose는 받는쪽에서 알아서 해줄 예정.
								var command = CMD_ENTITY_MOVE.Create();

								command.Id = netIdComponent.NetId;
								command.Time = DateTime.UtcNow.ToUnixTime();
								command.MoveType = MoveType.None;
								command.X = pos.x;
								command.Y = pos.y;
								command.Z = pos.z;
								command.VelocityX = 0;
								command.VelocityY = 0;
								command.VelocityZ = 0;

								clientService.SendCommand(command);
							}
						}
					}

					movementComponent.MoveDir = moveDirection;
					movementComponent.IsRun = inputComponent.IsRun;
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
