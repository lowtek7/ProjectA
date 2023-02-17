using System;
using Build.Stage;
using UnityEditor;
using UnityEngine;

namespace Build.Editor.Stage
{
	[CustomEditor(typeof(StageSetting))]
	public class StageSettingEditor : UnityEditor.Editor
	{
		private StageSetting current;

		public void OnEnable()
		{
			current = target as StageSetting;
		}

		public override void OnInspectorGUI()
		{
			var stageGuid = Guid.Empty;
			base.OnInspectorGUI();
			serializedObject.Update();
			using (new EditorGUILayout.HorizontalScope())
			{
				var guidProperty = serializedObject.FindProperty("stageGuid");
				stageGuid = current.StageGuid;

				EditorGUILayout.LabelField("Stage GUID", stageGuid.ToString());

				if (stageGuid == Guid.Empty && GUILayout.Button("Create"))
				{
					guidProperty.stringValue = Guid.NewGuid().ToString();
					serializedObject.ApplyModifiedProperties();
				}
				else if (stageGuid != Guid.Empty && GUILayout.Button("Copy"))
				{
					EditorGUIUtility.systemCopyBuffer = stageGuid.ToString();
				}
			}
			
			// if (stageGuid != Guid.Empty)
			// {
			// 	if (GUILayout.Button("Stage BUILD!"))
			// 	{
			// 		var result = StageBuilder.BuildToJson(current);
			// 	}
			// }
		}
	}
}
