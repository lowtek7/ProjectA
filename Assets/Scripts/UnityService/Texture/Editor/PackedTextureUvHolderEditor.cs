using UnityEditor;
using UnityEngine;

namespace UnityService.Texture.Editor
{
	[CustomEditor(typeof(PackedTextureUvHolder))]
	public class PackedTextureUvHolderEditor : UnityEditor.Editor
	{
		private PackedTextureUvHolder _holder;

		private void OnEnable()
		{
			_holder = target as PackedTextureUvHolder;
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.BeginVertical();

			EditorGUILayout.LabelField(nameof(PackedTextureUvHolder.textureSize), _holder.textureSize.ToString());
			EditorGUILayout.Space();

			EditorGUILayout.LabelField(nameof(PackedTextureUvHolder.uvs));

			var originIndentLevel = EditorGUI.indentLevel;

			for (int i = 0; i < _holder.uvs.Count; i++)
			{
				EditorGUI.indentLevel = originIndentLevel + 1;

				var textureUv = _holder.uvs[i];
				EditorGUILayout.LabelField($"Id : {i}", GUILayout.Width(50));

				EditorGUI.indentLevel = originIndentLevel + 2;

				using (new EditorGUI.DisabledScope(true))
				{
					EditorGUILayout.Vector2IntField("Start Pixel : ",
						new Vector2Int(textureUv.startX, textureUv.startY),
						GUILayout.Height(EditorGUIUtility.singleLineHeight));

					EditorGUILayout.ObjectField("", textureUv.originTexture,
						typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
				}

				EditorGUILayout.Space();
			}

			EditorGUI.indentLevel = originIndentLevel;

			EditorGUILayout.EndVertical();
		}
	}
}
