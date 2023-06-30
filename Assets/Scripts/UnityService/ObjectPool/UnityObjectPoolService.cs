using System;
using BlitzEcs;
using Library.JSPool;
using Service.ObjectPool;
using UnityEngine;

namespace UnityService.ObjectPool
{
	[UnityService(typeof(IObjectPoolService))]
	public class UnityObjectPoolService : MonoBehaviour, IObjectPoolService
	{
		[SerializeField]
		private PoolManager poolManager;

		public void Init(World world)
		{
		}

		public GameObject Spawn(Guid originalGuid, Vector3 position, Quaternion rotation, Transform parent = null) =>
			poolManager.Spawn(originalGuid, position, rotation, parent);

		public GameObject Spawn(Guid originalGuid, Vector3 position, Transform parent = null) =>
			poolManager.Spawn(originalGuid, position, parent);

		public GameObject Spawn(Guid originalGuid, Transform parent = null) =>
			poolManager.Spawn(originalGuid, parent);

		public T Spawn<T>(Guid originalGuid, Vector3 position, Quaternion rotation, Transform parent = null)
			where T : Component =>
			poolManager.Spawn<T>(originalGuid, position, rotation, parent);

		public T Spawn<T>(Guid originalGuid, Vector3 position, Transform parent = null)
			where T : Component =>
			poolManager.Spawn<T>(originalGuid, position, parent);

		public T Spawn<T>(Guid originalGuid, Transform parent = null)
			where T : Component =>
			poolManager.Spawn<T>(originalGuid, parent);

		public void Despawn(GameObject targetGameObject) =>
			poolManager.Despawn(targetGameObject);
	}
}
