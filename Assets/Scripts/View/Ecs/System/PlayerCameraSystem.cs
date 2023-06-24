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
			if (ServiceManager.TryGetService(out IPlayerCameraService instance))
			{
				foreach (var entity in query)
				{
					ref var transformComponent = ref entity.Get<TransformComponent>();

					instance.SetCameraPosition(transformComponent.Position);
				}
				
				foreach (var entity in inputQuery)
				{
					ref var inputComponent = ref entity.Get<InputComponent>();

					instance.SetCameraRotation(inputComponent.CameraRotation);
					instance.SetMouseClick(inputComponent.IsMouseClick);
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
