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

		public Vector3 ToCartesianPos()
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
