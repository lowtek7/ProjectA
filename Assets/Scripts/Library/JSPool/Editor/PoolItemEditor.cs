using System;
using UnityEditor;
using UnityEngine;

namespace Library.JSPool.Editor
{
	[CustomEditor(typeof(PoolItem))]
	public class PoolItemEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			var itemGuidProperty = serializedObject.FindProperty("itemGuid");
			var resultGuid = Guid.Empty;

			base.OnInspectorGUI();

			var resultText = "Error! Guid is Empty.";

			if (!string.IsNullOrEmpty(itemGuidProperty.stringValue) &&
				Guid.TryParse(itemGuidProperty.stringValue, out resultGuid))
			{
				resultText = itemGuidProperty.stringValue;
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("GUID", resultText);

				if (resultGuid != Guid.Empty && GUILayout.Button("Copy"))
				{
					EditorGUIUtility.systemCopyBuffer = resultGuid.ToString();
				}
			}
		}
	}
}
