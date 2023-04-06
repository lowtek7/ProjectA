using BlitzEcs;
using Service;
using UnityEngine;
using View.Service;

namespace UnityService.Camera
{
	[UnityService(typeof(IPlayerCameraService))]
	public class UnityPlayerCameraService : MonoBehaviour, IPlayerCameraService, IGameServiceCallback
	{
		[SerializeField]
		private UnityEngine.Camera playerCamera;

		private float zDistance = 0;

		/// <summary>
		/// z distance는 이미 정해져 있기 때문에 x, y만 받도록 한다
		/// </summary>
		/// <param name="position"></param>
		public void SetCameraPosition(Vector2 position)
		{
			playerCamera.transform.position = new Vector3(position.x, position.y, zDistance);
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
