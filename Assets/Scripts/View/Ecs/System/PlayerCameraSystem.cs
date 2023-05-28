using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Service;
using Service.Camera;

namespace View.Ecs.System
{
	public class PlayerCameraSystem : ISystem
	{
		private Query<PlayerCameraComponent, TransformComponent> query;

		public Order Order => Order.Highest;
		
		public void Init(World world)
		{
			query = new Query<PlayerCameraComponent, TransformComponent>(world);
		}

		public void Update(float deltaTime)
		{
			if (ServiceManager.TryGetService(out IPlayerCameraService instance))
			{
				foreach (var entity in query)
				{
					ref var transformComponent = ref entity.Get<TransformComponent>();

					instance.SetCameraPosition(transformComponent.Position);
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
