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
			private PoolItem item;

			[SerializeField]
			private int initialPoolSize = 10;

			public int InitialPoolSize => initialPoolSize;

			public PoolItem Item => item;
		}
		
		class PoolEntity
		{
			private PoolItem original;

			private Transform parent;

			public PoolItem[] pool;

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
		private List<PoolItemSetting> poolItems = new List<PoolItemSetting>();

		private readonly Dictionary<string, PoolEntity> poolEntities = new Dictionary<string, PoolEntity>();

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
					pool = new PoolItem[size]
				};
				entity.Parent.SetParent(transform);

				for (int i = 0; i < size; i++)
				{
					entity.pool[i] = GameObject.Instantiate(item, entity.Parent);
					entity.pool[i].gameObject.SetActive(false);
				}

				poolEntities.Add(item.ItemGuid, entity);
			}
		}
	}
}
