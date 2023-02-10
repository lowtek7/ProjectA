using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace Build.Stage
{
	public class StageSetting : MonoBehaviour
	{
		/// <summary>
		/// 이 객체의 근원의 유니크 ID
		/// </summary>
		[SerializeField, HideInInspector]
		private string stageGuid = string.Empty;

		public Guid StageGuid
		{
			get
			{
				if (Guid.TryParse(stageGuid, out var result))
				{
					return result;
				}

				return Guid.Empty;
			}
		}
		
#if UNITY_EDITOR
		void OnValidate()
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
							Undo.RecordObject(this, "Create Stage GUID");
							EditorUtility.SetDirty(gameObject);

							stageGuid = Guid.NewGuid().ToString();
						}
					}
				}
			}
		}
#endif
	}
}
