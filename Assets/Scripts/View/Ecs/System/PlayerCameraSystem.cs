using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Service;
using Service.Camera;
using UnityEngine;

namespace View.Ecs.System
{
	public class PlayerCameraSystem : ISystem
	{
		private Query<PlayerCameraComponent, TransformComponent> query;
		
		private Query<InputComponent> inputQuery;
		
		private bool createSpherial = false;

		public void Init(World world)
		{
			inputQuery = new (world);
			query = new Query<PlayerCameraComponent, TransformComponent>(world);
		}

		public void Update(float deltaTime)
		{
			if (ServiceManager.TryGetService<IPlayerCameraService>(out var cameraService))
			{
				foreach (var entity in query)
				{
					ref var cameraComponent = ref entity.Get<PlayerCameraComponent>();

					if (createSpherial == false)
					{
						createSpherial = true;

						cameraService.SetSpherical((cameraComponent.minRadius, cameraComponent.minRadius, cameraComponent.maxRadius),
							(cameraComponent.minAzimuthInRad, cameraComponent.minAzimuthInRad, cameraComponent.maxAzimuthInRad),
							(cameraComponent.minElevationInRad, cameraComponent.minElevationInRad, cameraComponent.maxElevationInRad));
					}

					cameraService.AddAzimuth(cameraComponent.MouseXDegree);
					cameraService.AddElevation(cameraComponent.MouseYDegree);
				}

				foreach (var moveEntity in query)
				{
					ref var transformComponent = ref moveEntity.Get<TransformComponent>();

					// 좀더 넓은 시야를 위해 플레이어의 약간 위쪽위치를 설정
					var position = transformComponent.Position + Vector3.up;

					// 카메라 줌아웃
					//playerCamera.transform.position = sphericalCoordinates.TranslateRadius(sw * Time.deltaTime * scrollSpeed).toCartesian + newPosition;

					cameraService.PlayerCameraTransform.position = position + cameraService.ToCartesianPos();

					cameraService.PlayerCameraTransform.LookAt(transformComponent.Position);
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
