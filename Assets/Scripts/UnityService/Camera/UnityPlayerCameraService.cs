using System;
using BlitzEcs;
using Game.Ecs.Component;
using Service;
using Service.Camera;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityService.Camera
{
	[UnityService(typeof(IPlayerCameraService))]
	public class UnityPlayerCameraService : MonoBehaviour, IPlayerCameraService
	{
		[SerializeField]
		private UnityEngine.Camera playerCamera;

		private Transform _playerCameraTransform;

		private Query<PlayerCameraComponent> _cameraQuery;

		private Query<MovementComponent, TransformComponent, PlayerComponent> _movementQuery;

		private bool isMouseClick = false;

		private bool createSpherial = false;

		private Vector3 targetPosition;

		private const float ROTCYCLE = 360f * Mathf.Deg2Rad;

		public Vector3 WorldSize
		{
			get
			{
				var cameraSize = playerCamera.orthographicSize;
				var aspect = playerCamera.aspect;
				var height = cameraSize * 2;

				return new Vector3(height * aspect, height, height * aspect);
			}
		}

		public Transform PlayerCameraTransform => _playerCameraTransform;

		void IPlayerCameraService.Fetch()
		{
			foreach (var entity in _cameraQuery)
			{
				ref var cameraComponent = ref entity.Get<PlayerCameraComponent>();

				if (createSpherial == false)
				{
					createSpherial = true;

					SetSpherical((cameraComponent.minRadius, cameraComponent.minRadius, cameraComponent.maxRadius),
						(cameraComponent.minAzimuthInRad, cameraComponent.minAzimuthInRad, cameraComponent.maxAzimuthInRad),
						(cameraComponent.minElevationInRad, cameraComponent.minElevationInRad, cameraComponent.maxElevationInRad));
				}

				AddAzimuth(cameraComponent.MouseXDegree);
				AddElevation(cameraComponent.MouseYDegree);
			}

			foreach (var moveEntity in _movementQuery)
			{
				ref var transformComponent = ref moveEntity.Get<TransformComponent>();

				// 좀더 넓은 시야를 위해 플레이어의 약간 위쪽위치를 설정
				var position = transformComponent.Position + Vector3.up;

				// 카메라 줌아웃
				//playerCamera.transform.position = sphericalCoordinates.TranslateRadius(sw * Time.deltaTime * scrollSpeed).toCartesian + newPosition;

				playerCamera.transform.position = position + ToCartesianPos();

				playerCamera.transform.LookAt(transformComponent.Position);
			}
		}

		//구면 카메라 코드
		public void SetSpherical(
			(float min, float init, float max) radius,
			(float min, float init, float max) azimuth,
			(float min, float init, float max) elevation)
		{
			foreach (var entity in _cameraQuery)
			{
				ref var cameraComponent = ref entity.Get<PlayerCameraComponent>();

				cameraComponent.minRadius = radius.min;
				cameraComponent.maxRadius = radius.max;
				SetRadius(radius.init);

				cameraComponent.minAzimuthInRad = azimuth.min * Mathf.Deg2Rad;
				cameraComponent.maxAzimuthInRad = azimuth.max * Mathf.Deg2Rad;

				cameraComponent.minElevationInRad = elevation.min * Mathf.Deg2Rad;
				cameraComponent.maxElevationInRad = elevation.max * Mathf.Deg2Rad;

				SetAzimuth(azimuth.init * Mathf.Deg2Rad);
				SetElevation(elevation.init * Mathf.Deg2Rad);
			}
		}

		public void SetAzimuth(float degree)
		{
			float value = degree * Mathf.Deg2Rad;

			if (value <= 0f)
			{
				value += ROTCYCLE;
			}

			if (value >= ROTCYCLE)
			{
				value -= ROTCYCLE;
			}

			foreach (var entity in _cameraQuery)
			{
				ref var cameraComponent = ref entity.Get<PlayerCameraComponent>();

				cameraComponent.Azimuth = Mathf.Clamp(value,
					cameraComponent.minAzimuthInRad,
					cameraComponent.maxAzimuthInRad);
			}
		}

		public void AddAzimuth(float degree)
		{
			foreach (var entity in _cameraQuery)
			{
				ref var cameraComponent = ref entity.Get<PlayerCameraComponent>();

				float addValue = cameraComponent.Azimuth + degree * Mathf.Deg2Rad;

				if (addValue <= 0f)
				{
					addValue += ROTCYCLE;
				}

				if (addValue > ROTCYCLE)
				{
					addValue -= ROTCYCLE;
				}

				cameraComponent.Azimuth = Mathf.Clamp(addValue,
					cameraComponent.minAzimuthInRad,
					cameraComponent.maxAzimuthInRad);
			}
		}

		public void SetElevation(float degree)
		{
			foreach (var entity in _cameraQuery)
			{
				ref var cameraComponent = ref entity.Get<PlayerCameraComponent>();

				cameraComponent.Elevation = Mathf.Clamp(degree * Mathf.Deg2Rad,
					cameraComponent.minElevationInRad,
					cameraComponent.maxElevationInRad);
			}
		}

		public void AddElevation(float degree)
		{
			foreach (var entity in _cameraQuery)
			{
				ref var cameraComponent = ref entity.Get<PlayerCameraComponent>();

				cameraComponent.Elevation = Mathf.Clamp(cameraComponent.Elevation + degree * Mathf.Deg2Rad,
					cameraComponent.minElevationInRad,
					cameraComponent.maxElevationInRad);
			}
		}

		public void SetRadius(float value)
		{
			foreach (var entity in _cameraQuery)
			{
				ref var cameraComponent = ref entity.Get<PlayerCameraComponent>();

				cameraComponent.Radius = Mathf.Clamp(value,
					cameraComponent.minRadius,
					cameraComponent.maxRadius);
			}
		}

		public void AddRadius(float value)
		{
			foreach (var entity in _cameraQuery)
			{
				ref var cameraComponent = ref entity.Get<PlayerCameraComponent>();

				cameraComponent.Radius = Mathf.Clamp(cameraComponent.Radius + value,
				cameraComponent.maxRadius,
					cameraComponent.maxRadius);
			}
		}

		private Vector3 ToCartesianPos()
		{
			Vector3 position = Vector3.zero;

			foreach (var entity in _cameraQuery)
			{
				ref var cameraComponent = ref entity.Get<PlayerCameraComponent>();

				position = new Vector3(
					cameraComponent.Radius
					* Mathf.Sin(cameraComponent.Elevation)
					* Mathf.Cos(cameraComponent.Azimuth),
					cameraComponent.Radius
					* Mathf.Cos(cameraComponent.Elevation),
					cameraComponent.Radius
					* Mathf.Sin(cameraComponent.Elevation)
					* Mathf.Sin(cameraComponent.Azimuth));
			}
			return position;
		}

		public Vector3 ScreenToWorld(Vector2 screenPos)
		{
			return playerCamera.ScreenToWorldPoint(screenPos);
		}

		public Vector2 WorldToScreen(Vector3 worldPos)
		{
			return playerCamera.WorldToScreenPoint(worldPos);
		}

		public void Init(World world)
		{
			_cameraQuery = new Query<PlayerCameraComponent>(world);
			_movementQuery = new (world);
			_playerCameraTransform = playerCamera.transform;
		}

		private void Awake()
		{
		}


		public void OnActivate()
		{

		}

		public void OnDeactivate()
		{

		}
	}
}
