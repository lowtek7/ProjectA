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
			base.OnInspectorGUI();
			serializedObject.Update();
			using (new EditorGUILayout.HorizontalScope())
			{
				var guidProperty = serializedObject.FindProperty("stageGuid");
				var guid = current.StageGuid;

				EditorGUILayout.LabelField("Stage GUID", guid.ToString());

				if (guid == Guid.Empty && GUILayout.Button("Create"))
				{
					guidProperty.stringValue = Guid.NewGuid().ToString();
					serializedObject.ApplyModifiedProperties();
				}
				else if (guid != Guid.Empty && GUILayout.Button("Copy"))
				{
					EditorGUIUtility.systemCopyBuffer = guid.ToString();
				}
			}
		}
	}
}
