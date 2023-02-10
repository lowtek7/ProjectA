using System;
using System.Collections.Generic;
using UnityEngine;

namespace Library.JSPool
{
	/// <summary>
	/// 풀링을 관리하는 모노비헤이비어
	/// 기본적으로 addressable asset를 사용한다.
	/// </summary>
	public class PoolManager : MonoBehaviour
	{
		[Serializable]
		class PoolItemSetting
		{
			[SerializeField]
			private GameObject prefab;

			[SerializeField]
			private int initialPoolSize = 10;

			public int InitialPoolSize => initialPoolSize;

			public PoolItem Item => prefab.GetComponent<PoolItem>();
		}
		
		class PoolEntity
		{
			private PoolItem original;

			private Transform parent;

			public Queue<PoolItem> pool;

			public PoolItem Original => original;

			public Transform Parent => parent;

			public PoolEntity(string name, PoolItem original)
			{
				this.original = original;
				var go = new GameObject(name);
				parent = go.transform;
			}
		}

		[SerializeField]
		private List<PoolItemSetting> poolItems = new ();

		private readonly Dictionary<Guid, PoolEntity> poolEntities = new ();

		/// <summary>
		/// 풀 초기화 작업
		/// 객체들 미리 풀링해두는 함수
		/// 절대 두번 호출해서는 안된다.
		/// </summary>
		public void Init()
		{
			if (poolEntities.Count > 0)
			{
				Debug.LogError("Error! PoolEntities is already is use!");
				poolEntities.Clear();
			}

			foreach (var poolItem in poolItems)
			{
				var item = poolItem.Item;
				var size = poolItem.InitialPoolSize;
				var entity = new PoolEntity($"{item.name} ({item.ItemGuid})", item)
				{
					pool = new Queue<PoolItem>()
				};
				entity.Parent.SetParent(transform);

				for (int i = 0; i < size; i++)
				{
					var result = GameObject.Instantiate(item, entity.Parent);
					result.gameObject.SetActive(false);
					entity.pool.Enqueue(result);
				}

				poolEntities.Add(item.ItemGuid, entity);
			}
		}

		public void Despawn(GameObject targetGameObject)
		{
			if (targetGameObject.TryGetComponent<PoolItem>(out var poolItem))
			{
				if (targetGameObject.TryGetComponent<PoolController>(out var poolController))
				{
					poolController.DespawnEvent();
				}

				var guid = poolItem.ItemGuid;
				if (poolEntities.TryGetValue(guid, out var poolEntity))
				{
					poolEntity.pool.Enqueue(poolItem);
					poolItem.transform.SetParent(poolEntity.Parent);
					targetGameObject.SetActive(false);
				}
			}
		}
		
		public GameObject Spawn(Guid originalGuid, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if (poolEntities.TryGetValue(originalGuid, out var poolEntity))
			{
				if (poolEntity.pool.Count == 0)
				{
					var poolParent = poolEntity.Parent;
					for (int i = 0; i < 10; i++)
					{
						var result = GameObject.Instantiate(poolEntity.Original, poolParent);
						result.gameObject.SetActive(false);
						poolEntity.pool.Enqueue(result);
					}
				}

				var item = poolEntity.pool.Dequeue();
				item.transform.SetParent(parent);
				item.transform.position = position;
				item.transform.rotation = rotation;
				item.gameObject.SetActive(true);

				if (item.gameObject.TryGetComponent<PoolController>(out var poolController))
				{
					poolController.SpawnEvent();
				}

				return item.gameObject;
			}
			
			return null;
		}

		public GameObject Spawn(Guid originalGuid, Vector3 position, Transform parent = null) => Spawn(originalGuid, position, Quaternion.identity, parent);

		public GameObject Spawn(Guid originalGuid, Transform parent = null) => Spawn(originalGuid, Vector3.zero, Quaternion.identity, parent);
	}
}
