using System;
using System.Collections.Generic;
using BlitzEcs;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Game.Unit
{
	/// <summary>
	/// Entity 관련 프리팹 만들때 이것을 무조건 부착 시켜야한다.
	/// </summary>
	public class UnitTemplate : MonoBehaviour
	{
		/// <summary>
		/// 이 객체의 근원의 유니크 ID
		/// </summary>
		[SerializeField, HideInInspector]
		private string sourceGuid = string.Empty;

		public Guid SourceGuid
		{
			get
			{
				if (Guid.TryParse(sourceGuid, out var result))
				{
					return result;
				}

				return Guid.Empty;
			}
		}

		private readonly List<IUnitBehaviour> UnitBehaviours = new List<IUnitBehaviour>();

		/// <summary>
		/// 엔티티와 동기화 하는 작업
		/// </summary>
		public void Connect(Entity entity)
		{
			// UnitBehaviours가 비어있다면 수집해 준다.
			if (UnitBehaviours.Count == 0)
			{
				UnitBehaviours.AddRange(gameObject.GetComponents<IUnitBehaviour>());
			}

			foreach (var unitBehaviour in UnitBehaviours)
			{
				unitBehaviour.Connect(entity);
			}
		}

		public void Disconnect()
		{
			// UnitBehaviours가 비어있다면 수집해 준다.
			if (UnitBehaviours.Count == 0)
			{
				UnitBehaviours.AddRange(gameObject.GetComponents<IUnitBehaviour>());
			}

			foreach (var unitBehaviour in UnitBehaviours)
			{
				unitBehaviour.Disconnect();
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
						if (Selection.activeObject is GameObject go &&
							go.TryGetComponent(out UnitTemplate unitTemplate) &&
							PrefabUtility.GetPrefabAssetType(gameObject) == PrefabAssetType.Regular &&
							PrefabUtility.GetPrefabInstanceStatus(gameObject) == PrefabInstanceStatus.NotAPrefab)
						{
							Undo.RecordObject(this, "Create Source GUID");
							EditorUtility.SetDirty(gameObject);
							
							sourceGuid = Guid.NewGuid().ToString();
						}
					}
				}
			}
		}
#endif
	}
}
