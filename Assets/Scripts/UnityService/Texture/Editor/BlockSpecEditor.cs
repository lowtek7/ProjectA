using UnityEditor;
using UnityEngine;

namespace UnityService.Texture.Editor
{
	[CustomPropertyDrawer(typeof(BlockSpec))]
	public class BlockSpecDrawer : UnityEditor.PropertyDrawer
	{
		static readonly string[] DirectionStr =
		{
			"back",
			"right",
			"forward",
			"left",
			"down",
			"up",
		};

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return (16 + 4) * (1 + 1 + 6);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var prevPos = new Rect(position.position.x, position.position.y + 2, position.width, 16);
			EditorGUI.PropertyField(prevPos, property.FindPropertyRelative(nameof(BlockSpec.name)));

			prevPos = new Rect(position.x, prevPos.yMax + 4, position.width, 16);

			EditorGUI.PropertyField(prevPos, property.FindPropertyRelative(nameof(BlockSpec.blockId)));

			var textureArrayProp = property.FindPropertyRelative(nameof(BlockSpec.textures));

			if (textureArrayProp.arraySize == 0)
			{
				textureArrayProp.arraySize = 6;
			}

			for (int i = 0; i < textureArrayProp.arraySize; i++)
			{
				prevPos = new Rect(position.x, prevPos.yMax + 4, position.width, 16);

				EditorGUI.PropertyField(prevPos, textureArrayProp.GetArrayElementAtIndex(i), new GUIContent(DirectionStr[i]));
			}

			EditorGUI.EndProperty();
		}
	}
}
