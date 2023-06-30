using System;
using System.Collections.Generic;
using UnityEngine;

namespace Library.JSPool
{
	/// <summary>
	/// 풀링을 관리하는 모노비헤이비어
	/// 기본적으로 addressable asset를 사용한다.
	/// </summary>
	public class PoolManager : MonoBehaviour, IDisposable
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

			private Queue<PoolItem> pool;

			/// <summary>
			/// 지금까지 생성되었던 모든 아이템들
			/// </summary>
			private List<PoolItem> allItems;

			public List<PoolItem> AllItems => allItems;

			public PoolItem Original => original;

			public Transform Parent => parent;

			public int PoolCount => pool.Count;

			public PoolEntity(string name, PoolItem original)
			{
				this.original = original;
				var go = new GameObject(name);
				parent = go.transform;
				pool = new Queue<PoolItem>();
				allItems = new List<PoolItem>();
			}

			public void Register(PoolItem item)
			{
				pool.Enqueue(item);
				allItems.Add(item);
			}

			public PoolItem Dequeue()
			{
				return pool.Dequeue();
			}

			public void Return(PoolItem item)
			{
				pool.Enqueue(item);
			}
		}

		[SerializeField]
		private List<PoolItemSetting> poolItems = new ();

		private readonly Dictionary<Guid, PoolEntity> poolEntities = new ();

		private readonly Dictionary<Guid, Dictionary<PoolItem, Dictionary<Type, Component>>> cachedComponents = new ();

		/// <summary>
		/// 풀 초기화 작업
		/// 객체들 미리 풀링해두는 함수
		/// 절대 두번 호출해서는 안된다.
		/// </summary>
		public void Init()
		{
			cachedComponents.Clear();

			if (poolEntities.Count > 0)
			{
				Debug.LogError("Error! PoolEntities is already is use!");
				poolEntities.Clear();
			}

			foreach (var poolItem in poolItems)
			{
				var item = poolItem.Item;
				var size = poolItem.InitialPoolSize;
				var entity = new PoolEntity($"{item.name} ({item.ItemGuid})", item);
				entity.Parent.SetParent(transform);

				for (int i = 0; i < size; i++)
				{
					var result = GameObject.Instantiate(item, entity.Parent);
					result.gameObject.SetActive(false);
					entity.Register(result);
				}

				poolEntities.Add(item.ItemGuid, entity);
			}
		}

		public void Despawn(GameObject targetGameObject)
		{
			if (targetGameObject.TryGetComponent<PoolItem>(out var poolItem))
			{
				poolItem.DespawnEvent();

				var guid = poolItem.ItemGuid;
				if (poolEntities.TryGetValue(guid, out var poolEntity))
				{
					poolEntity.Return(poolItem);
					poolItem.transform.SetParent(poolEntity.Parent);
					targetGameObject.SetActive(false);
				}
			}
		}

		private PoolItem SpawnInternal(Guid originalGuid, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if (poolEntities.TryGetValue(originalGuid, out var poolEntity))
			{
				if (poolEntity.PoolCount == 0)
				{
					var poolParent = poolEntity.Parent;
					for (int i = 0; i < 10; i++)
					{
						var result = GameObject.Instantiate(poolEntity.Original, poolParent);
						result.gameObject.SetActive(false);
						poolEntity.Register(result);
					}
				}

				var item = poolEntity.Dequeue();
				item.transform.SetParent(parent);
				item.transform.position = position;
				item.transform.rotation = rotation;
				item.gameObject.SetActive(true);

				if (item.gameObject.TryGetComponent<PoolItem>(out var poolItem))
				{
					poolItem.SpawnEvent();
				}

				return item;
			}

			return null;
		}

		public GameObject Spawn(Guid originalGuid, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			return SpawnInternal(originalGuid, position, rotation, parent).gameObject;
		}

		public GameObject Spawn(Guid originalGuid, Vector3 position, Transform parent = null) => Spawn(originalGuid, position, Quaternion.identity, parent);

		public GameObject Spawn(Guid originalGuid, Transform parent = null) => Spawn(originalGuid, Vector3.zero, Quaternion.identity, parent);

		public T Spawn<T>(Guid originalGuid, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
		{
			var item = SpawnInternal(originalGuid, position, rotation, parent);

			StartCache<T>(originalGuid);

			if (cachedComponents.TryGetValue(originalGuid, out var result) &&
				result.TryGetValue(item, out var components) &&
				components.TryGetValue(typeof(T), out var component) &&
				component is T output)
			{
				return output;
			}

#if UNITY_EDITOR
			Debug.LogError($"Spawn<T> Failed. {typeof(T).FullName} GetComponent Failed! Original Guid : {originalGuid}");
#endif

			Despawn(item.gameObject);
			return null;
		}

		public T Spawn<T>(Guid originalGuid, Vector3 position, Transform parent = null) where T : Component => Spawn<T>(originalGuid, position, Quaternion.identity, parent);

		public T Spawn<T>(Guid originalGuid, Transform parent = null) where T : Component => Spawn<T>(originalGuid, Vector3.zero, Quaternion.identity, parent);

		public void Dispose()
		{
			cachedComponents.Clear();
		}

		/// <summary>
		/// 해당 guid의 풀링된 엔티티의 컴포넌트들을 캐시를 시작하게 하는 것.
		/// 만약 기존에 풀링 된 것들이 있다면 없는 애들만 넣는다.
		/// </summary>
		/// <param name="originalGuid"></param>
		private void StartCache<T>(Guid originalGuid) where T : Component
		{
			if (!cachedComponents.TryGetValue(originalGuid, out var result))
			{
				result = new Dictionary<PoolItem, Dictionary<Type, Component>>();
				cachedComponents[originalGuid] = result;
			}

			var type = typeof(T);

			if (poolEntities.TryGetValue(originalGuid, out var entity))
			{
				foreach (var item in entity.AllItems)
				{
					if (!result.TryGetValue(item, out var components))
					{
						components = new Dictionary<Type, Component>();
						result[item] = components;
					}

					// 캐싱되어 있지 않다면 캐싱해준다.
					if (!components.ContainsKey(type))
					{
						components[type] = item.GetComponent<T>();
					}
				}
			}
		}
	}
}
