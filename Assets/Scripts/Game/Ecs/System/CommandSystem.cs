using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Service;
using Service.Command;

namespace Game.Ecs.System
{
	public class CommandSystem : ISystem
	{
		private Query<CommandComponent> _query;

		public Order Order => Order.Highest;
		
		public void Init(BlitzEcs.World world)
		{
			_query = new Query<CommandComponent>(world);
		}

		public void Update(float deltaTime)
		{
			if (ServiceManager.TryGetService<IGameCommandService>(out var gameCommandService))
			{
				foreach (var entity in _query)
				{
					ref var commandComponent = ref entity.Get<CommandComponent>();
					var commandType = commandComponent.TypeId;
					var magnitude = commandComponent.Magnitude;

					if (gameCommandService.TryGet(commandType, out var gameCommand))
					{
						gameCommand.Execute(entity);
					}
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
