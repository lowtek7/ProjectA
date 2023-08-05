using System;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Tool.Stage
{
	public class StageSetting : MonoBehaviour
	{
		/// <summary>
		/// 이 객체의 근원의 유니크 ID
		/// </summary>
		[SerializeField, HideInInspector]
		private string stageGuid = string.Empty;

		/// <summary>
		/// 절차적 생성 기능 사용 여부
		/// </summary>
		[SerializeField]
		private bool useProceduralGen = false;

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

		public bool UseProceduralGen => useProceduralGen;

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
						if (Selection.activeObject is GameObject go &&
							go.TryGetComponent(out StageSetting stageSetting) &&
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
