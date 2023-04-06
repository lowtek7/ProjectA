using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Service;
using Service.Input;

namespace Game.Ecs.System
{
	public class InputSystem : ISystem
	{
		public int Order => 99;

		private Query<InputComponent> inputQuery;
		private Query<MovementComponent> movementQuery;

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

				movementQuery.ForEach((ref MovementComponent movementComponent) =>
				{
					movementComponent.MoveDir = moveDirection;
				});
			});
		}
	}
}
