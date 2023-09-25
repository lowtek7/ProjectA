using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Game.Extensions;
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
				float mouseXDegree = 0.0f;
				float mouseYDegree = 0.0f;
				foreach (var inputEntity in inputQuery)
				{
					ref var inputComponent = ref inputEntity.Get<InputComponent>();

					mouseXDegree = inputComponent.MouseXYDegree.x;
					mouseYDegree = inputComponent.MouseXYDegree.y;
				}
				
				foreach (var cameraEntity in query)
				{
					if (cameraEntity.IsRemoteEntity())
					{
						continue;
					}

					ref var cameraComponent = ref cameraEntity.Get<PlayerCameraComponent>();

					if (createSpherial == false)
					{
						createSpherial = true;

						cameraService.SetSpherical((cameraComponent.minRadius, cameraComponent.minRadius, cameraComponent.maxRadius),
							(cameraComponent.minAzimuthInRad, cameraComponent.minAzimuthInRad, cameraComponent.maxAzimuthInRad),
							(cameraComponent.minElevationInRad, cameraComponent.minElevationInRad, cameraComponent.maxElevationInRad));
					}

					cameraService.AddAzimuth(mouseXDegree);
					cameraService.AddElevation(mouseYDegree);
				}

				foreach (var moveEntity in query)
				{
					if (moveEntity.IsRemoteEntity())
					{
						continue;
					}

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
