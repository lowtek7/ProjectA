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
		
		private Query<InputComponent> inputQuery;

		public void Init(World world)
		{
			inputQuery = new (world);
			query = new Query<PlayerCameraComponent, TransformComponent>(world);
		}

		public void Update(float deltaTime)
		{
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
