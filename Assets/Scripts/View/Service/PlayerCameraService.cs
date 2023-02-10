using BlitzEcs;
using Game.Service;
using UnityEngine;

namespace View.Service
{
	public class PlayerCameraService : MonoBehaviour, IGameService
	{
		private static bool instanceFlag = false;
		private static PlayerCameraService instance;

		/// <summary>
		/// 모노비헤이비어 널 체크는 퍼포먼스적으로 큰 작업이기 때문에 (라이더가 알려줌)
		/// 인스턴스의 널체크를 instanceFlag를 이용해 대신 한다
		/// </summary>
		/// <param name="playerCameraService"></param>
		/// <returns></returns>
		public static bool TryGetInstance(out PlayerCameraService playerCameraService)
		{
			playerCameraService = instance;
			return instanceFlag;
		}

		[SerializeField]
		private Camera camera;

		private float zDistance = 0;

		/// <summary>
		/// z distance는 이미 정해져 있기 때문에 x, y만 받도록 한다
		/// </summary>
		/// <param name="position"></param>
		public void SetCameraPosition(Vector2 position)
		{
			camera.transform.position = new Vector3(position.x, position.y, zDistance);
		}

		public void Init(World world)
		{
		}

		private void Awake()
		{
			instance = this;
			instanceFlag = true;
			zDistance = camera.transform.position.z;
		}

		private void OnDestroy()
		{
			instance = null;
			instanceFlag = false;
		}
	}
}
