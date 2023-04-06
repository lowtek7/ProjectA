using BlitzEcs;
using Service;
using UnityEngine;
using View.Service;

namespace UnityService.Camera
{
	public class UnityPlayerCameraService : MonoBehaviour, IPlayerCameraService
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
			
			// 일단 임시적으로 여기서 등록해주자...
			if (Application.isPlaying)
			{
				ServiceManager.SetService(typeof(IPlayerCameraService), this);
			}
		}

		private void OnDestroy()
		{
			// 임시적으로 여기서 제거한다.
			if (Application.isPlaying)
			{
				ServiceManager.ClearService(typeof(IPlayerCameraService));
			}
		}
	}
}
