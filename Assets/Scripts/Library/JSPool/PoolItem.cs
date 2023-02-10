using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Library.JSPool
{
	public class PoolItem : MonoBehaviour
	{
		[SerializeField, HideInInspector]
		private string itemGuid = string.Empty;

		private Guid itemGuidInternal;

		public Guid ItemGuid
		{
			get
			{
				if (itemGuidInternal == Guid.Empty)
				{
					itemGuidInternal = Guid.Parse(itemGuid);
				}

				return itemGuidInternal;
			}
		}

		/// <summary>
		/// 풀 매니저를 제외하고 다른데서 호출하는 행위 절대 금지.
		/// </summary>
		public void SpawnEvent()
		{
			var components = gameObject.GetComponents<IPoolEvent>();

			foreach (var poolEvent in components)
			{
				poolEvent.OnSpawned();
			}
		}
		
		/// <summary>
		/// 풀 매니저를 제외하고 다른데서 호출하는 행위 절대 금지.
		/// </summary>
		public void DespawnEvent()
		{
			var components = gameObject.GetComponents<IPoolEvent>();

			foreach (var poolEvent in components)
			{
				poolEvent.OnDespawned();
			}
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (string.IsNullOrEmpty(itemGuid))
			{
				itemGuid = Guid.NewGuid().ToString();
				Undo.RecordObject(gameObject, "PoolItem Set Guid");
				EditorUtility.SetDirty(gameObject);
			}
		}
#endif
	}
}
