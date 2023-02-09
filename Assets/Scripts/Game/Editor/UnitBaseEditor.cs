﻿using System;
using Core.Utility;
using Game.Unit;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	[CustomEditor(typeof(UnitBase))]
	public class UnitBaseEditor : UnityEditor.Editor
	{
		private UnitBase current;
		
		public void OnEnable()
		{
			current = target as UnitBase;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();
			using (new EditorGUILayout.HorizontalScope())
			{
				var guidProperty = serializedObject.FindProperty("sourceGuid");
				var guid = current.SourceGuid;

				EditorGUILayout.LabelField("Source GUID", guid.ToString());

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
