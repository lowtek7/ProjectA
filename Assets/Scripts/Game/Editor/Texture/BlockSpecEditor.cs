using System;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Texture
{
	[CustomPropertyDrawer(typeof(BlockSpec))]
	public class BlockSpecDrawer : UnityEditor.PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return (16 + 4) * (1 + 6);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var prevPos = new Rect(position.position.x, position.position.y + 2, position.width, 16);
			EditorGUI.PropertyField(prevPos, property.FindPropertyRelative(nameof(BlockSpec.name)));

			var textureArrayProp = property.FindPropertyRelative(nameof(BlockSpec.textures));

			for (int i = 0; i < textureArrayProp.arraySize; i++)
			{
				prevPos = new Rect(position.x, prevPos.yMax + 4, position.width, 16);

				EditorGUI.PropertyField(prevPos, textureArrayProp.GetArrayElementAtIndex(i));
			}

			EditorGUI.EndProperty();
		}
	}
}
