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

		public string ItemGuid => itemGuid;

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
