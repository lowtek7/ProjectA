using System;
using UnityEditor;

namespace Library.JSPool.Editor
{
	[CustomEditor(typeof(PoolItem))]
	public class PoolItemEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			var itemGuidProperty = serializedObject.FindProperty("itemGuid");
			
			base.OnInspectorGUI();

			var resultText = "Error! Guid is Empty.";

			if (!string.IsNullOrEmpty(itemGuidProperty.stringValue) &&
				Guid.TryParse(itemGuidProperty.stringValue, out var guid))
			{
				resultText = itemGuidProperty.stringValue;
			}
			
			EditorGUILayout.LabelField("GUID", resultText);
		}
	}
}
