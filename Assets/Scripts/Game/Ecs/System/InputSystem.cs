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
		public Order Order => Order.Highest;

		private Query<InputComponent> inputQuery;

		private Query<MovementComponent, TransformComponent, PlayerComponent> movementQuery;

		public void Init(BlitzEcs.World world)
		{
			inputQuery = new (world);
			movementQuery = new (world);
		}

		public void Update(float deltaTime)
		{
			if (ServiceManager.TryGetService<IGameInputService>(out var inputService))
			{
				inputService.Fetch();
			}

			inputQuery.ForEach((ref InputComponent inputComponent) =>
			{
				var moveDirection = inputComponent.MoveDirection;

				movementQuery.ForEach((ref MovementComponent movementComponent, ref TransformComponent transformComponent, ref PlayerComponent _) =>
				{
					// 임시적으로 디렉션을 무브 디렉션을 사용하게 하자.
					if (moveDirection != Vector3.zero)
					{
						transformComponent.Direction = moveDirection;
					}

					movementComponent.MoveDir = moveDirection;
				});
			});
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
