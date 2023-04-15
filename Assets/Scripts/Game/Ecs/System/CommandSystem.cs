using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Service;
using Service.Command;

namespace Game.Ecs.System
{
	public class CommandSystem : ISystem
	{
		private Query<CommandComponent> query;

		public Order Order => Order.Highest;
		
		public void Init(BlitzEcs.World world)
		{
			query = new Query<CommandComponent>(world);
		}

		public void Update(float deltaTime)
		{
			if (ServiceManager.TryGetService<IGameCommandService>(out var gameCommandService))
			{
				query.ForEach((Entity entity, ref CommandComponent commandComponent) =>
				{
					var commandType = commandComponent.Type;
					var magnitude = commandComponent.Magnitude;

					if (gameCommandService.TryGet(commandType, out var gameCommand))
					{
					}
				});
			}
		}
	}
}
