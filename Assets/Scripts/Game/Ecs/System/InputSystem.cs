using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Service;
using Service.Input;
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
					ref var movementComponent = ref moveEntity.Get<MovementComponent>();
					ref var transformComponent = ref moveEntity.Get<TransformComponent>();

					// 임시적으로 디렉션을 무브 디렉션을 사용하게 하자.
					if (moveDirection != Vector3.zero)
					{
						transformComponent.Direction = moveDirection;
						movementComponent.TargetRotation = Quaternion.LookRotation(moveDirection);
					}

					movementComponent.MoveDir = moveDirection;
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
