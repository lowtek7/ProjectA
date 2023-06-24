using BlitzEcs;
using Service;
using Service.Camera;
using UnityEngine;

namespace UnityService.Camera
{
	[UnityService(typeof(IPlayerCameraService))]
	public class UnityPlayerCameraService : MonoBehaviour, IPlayerCameraService, IGameServiceCallback
	{
		[SerializeField]
		private UnityEngine.Camera playerCamera;

		private float yDistance = 0;
		
		private float zDistance = 0;

		private bool isMouseClick = false;

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

		/// <summary>
		/// 3차원에서 z와 y에 오프셋 거리만 큼 떨어뜨린 후, 바라보게 한다
		/// </summary>
		/// <param name="targetPosition"></param>
		public void SetCameraPosition(Vector3 targetPosition)
		{
			playerCamera.transform.position = new Vector3(
					targetPosition.x,
					targetPosition.y + yDistance,
					targetPosition.z + zDistance
					);
			if (isMouseClick == false)
			{
				playerCamera.transform.LookAt(targetPosition);	
			}
		}
		
		public void SetCameraRotation(Vector2 rotation)
		{
			Vector3 cameraPosition = playerCamera.transform.position;

			playerCamera.transform.RotateAround(cameraPosition, Vector3.up, rotation.x);
		}

		public void SetMouseClick(bool mouseClick)
		{
			isMouseClick = mouseClick;
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
		}

		private void Awake()
		{
			yDistance = playerCamera.transform.position.y;
			zDistance = playerCamera.transform.position.z;
		}

		public void OnActivate()
		{
		}

		public void OnDeactivate()
		{
		}
	}
}
