using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Service;
using Service.Network;

namespace Game.Ecs.System
{
	public class NetworkEntitySystem : ISystem
	{
		private Query<PlayerComponent> playerQuery;

		public void Init(BlitzEcs.World world)
		{
			playerQuery = new Query<PlayerComponent>(world);
		}

		public void Update(float deltaTime)
		{
			if (ServiceManager.TryGetService(out INetClientService clientService))
			{
				// foreach (var entity in playerQuery)
				// {
				// }
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
