using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using View.Service;

namespace View.Ecs.System
{
	public class PlayerCameraSystem : ISystem
	{
		private Query<PlayerCameraComponent, TransformComponent> query;

		public int Order => 1;
		
		public void Init(World world)
		{
			query = new Query<PlayerCameraComponent, TransformComponent>(world);
		}

		public void Update(float deltaTime)
		{
			if (PlayerCameraService.TryGetInstance(out var instance))
			{
				query.Fetch();
				query.ForEach((ref PlayerCameraComponent playerCameraComponent,
					ref TransformComponent transformComponent) =>
				{
					instance.SetCameraPosition(transformComponent.Position);
				});
			}
		}
	}
}
