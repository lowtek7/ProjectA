using System;
using UnityEngine;

namespace Service.ObjectPool
{
	/// <summary>
	/// 오브젝트 풀 서비스
	/// </summary>
	public interface IObjectPoolService : IGameService
	{
		GameObject Spawn(Guid originalGuid, Vector3 position, Quaternion rotation, Transform parent = null);

		GameObject Spawn(Guid originalGuid, Vector3 position, Transform parent = null);

		GameObject Spawn(Guid originalGuid, Transform parent = null);

		T Spawn<T>(Guid originalGuid, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component;

		T Spawn<T>(Guid originalGuid, Vector3 position, Transform parent = null) where T : Component;

		T Spawn<T>(Guid originalGuid, Transform parent = null) where T : Component;

		void Despawn(GameObject targetGameObject);
	}
}
