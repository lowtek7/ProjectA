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

		private IPoolEvent[] _poolEvents = null;

		private void Awake()
		{
			_poolEvents = gameObject.GetComponents<IPoolEvent>();
		}

		/// <summary>
		/// 풀 매니저를 제외하고 다른데서 호출하는 행위 절대 금지.
		/// </summary>
		public void SpawnEvent()
		{
			foreach (var poolEvent in _poolEvents)
			{
				poolEvent.OnSpawned();
			}
		}
		
		/// <summary>
		/// 풀 매니저를 제외하고 다른데서 호출하는 행위 절대 금지.
		/// </summary>
		public void DespawnEvent()
		{
			foreach (var poolEvent in _poolEvents)
			{
				poolEvent.OnDespawned();
			}
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (string.IsNullOrEmpty(itemGuid))
			{
				Undo.RecordObject(gameObject, "PoolItem Set Guid");
				EditorUtility.SetDirty(gameObject);

				itemGuid = Guid.NewGuid().ToString();
			}
			else
			{
				Event e = Event.current;

				if (e != null)
				{
					if (Application.isEditor)
					{
						if (e.type == EventType.Used && (e.commandName == "Duplicate" || e.commandName == "Paste"))
						{
							if (Selection.activeObject is GameObject &&
								PrefabUtility.GetPrefabAssetType(gameObject) == PrefabAssetType.Regular &&
								PrefabUtility.GetPrefabInstanceStatus(gameObject) == PrefabInstanceStatus.NotAPrefab)
							{
								Undo.RecordObject(this, "Create Item GUID");
								EditorUtility.SetDirty(gameObject);

								itemGuid = Guid.NewGuid().ToString();
							}
						}
					}
				}
			}
		}
#endif
	}
}
