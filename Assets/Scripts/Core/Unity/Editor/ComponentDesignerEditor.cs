using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Core.Unity.Editor
{
	[CustomEditor(typeof(ComponentDesigner))]
	public class ComponentDesignerEditor : UnityEditor.Editor
	{
		private static bool isInit = false;

		private static readonly Dictionary<string, Type> componentTypes = new Dictionary<string, Type>();

		private ComponentDesigner designer = null;
		
		private void OnEnable()
		{
			designer = (ComponentDesigner)target;
		}

		public override void OnInspectorGUI()
		{
			if (!isInit)
			{
				Init();
			}

			var components = designer.Components;

			foreach (var keyValue in componentTypes)
			{
				EditorGUILayout.LabelField(keyValue.Key);
			}
		}

		private void Init()
		{
			componentTypes.Clear();
			var types = TypeCache.GetTypesWithAttribute<ComponentDescriptionAttribute>();

			foreach (var type in types)
			{
				if (!string.IsNullOrEmpty(type.FullName))
				{
					var descriptionAttribute = type.GetCustomAttribute<ComponentDescriptionAttribute>();
					var displayName = type.FullName;

					if (!string.IsNullOrEmpty(descriptionAttribute.DisplayName))
					{
						displayName = descriptionAttribute.DisplayName;
					}

					componentTypes.Add(displayName, type);
				}
			}

			isInit = true;
		}
	}
}
