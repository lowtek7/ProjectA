using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Library.JSAnim2D.Editor
{
	/// <summary>
	/// 임시용 애니메이션 클론 기능
	/// 나중에 애니메이션 에디터 자체에 통합 예정
	/// </summary>
	public class JSSpriteAnimationCloneEditor : EditorWindow
	{
		private JSSpriteAnimationData baseData;

		private Texture2D[] targetDataList = Array.Empty<Texture2D>();

		private string outputPrefix = string.Empty;

		private string outputPostfix = string.Empty;

		private string outputPath = "Assets";

		private void OnGUI()
		{
			baseData =
				EditorGUILayout.ObjectField("Base", baseData, typeof(JSSpriteAnimationData), true) as
					JSSpriteAnimationData;

			using (new EditorGUILayout.HorizontalScope())
			{
				var count = EditorGUILayout.IntField("Target Texture Count", targetDataList.Length);

				if (count != targetDataList.Length)
				{
					var tempArray = new Texture2D[count];
					Array.Copy(targetDataList, tempArray,
						count > targetDataList.Length ? targetDataList.Length : count);
					targetDataList = tempArray;
				}
			}

			EditorGUILayout.LabelField("Target Textures");
			for (int i = 0; i < targetDataList.Length; i++)
			{
				targetDataList[i] =
					EditorGUILayout.ObjectField($"[{i}]", targetDataList[i], typeof(Texture2D), true) as Texture2D;
			}

			outputPrefix = EditorGUILayout.TextField("Output Prefix", outputPrefix);
			outputPostfix = EditorGUILayout.TextField("Output Postfix", outputPostfix);
			outputPath = EditorGUILayout.TextField("Output Path", outputPath);

			if (GUILayout.Button("Apply") && baseData != null && targetDataList.Length > 0)
			{
				var dataMap = new Dictionary<Texture2D, JSSpriteAnimationData>();

				foreach (var tex in targetDataList)
				{
					var data = ScriptableObject.CreateInstance<JSSpriteAnimationData>();
					data.name = string.Empty;

					if (!string.IsNullOrEmpty(outputPrefix))
					{
						data.name = outputPrefix + "_";
					}
					
					data.name += tex.name + "_" + outputPostfix;
					dataMap.Add(tex, data);
				}

				foreach (var animationInfo in baseData.Animations)
				{
					foreach (var keyValuePair in dataMap)
					{
						var targetTexture = keyValuePair.Key;
						var targetData = keyValuePair.Value;
						var targetAnimationInfo =
							AnimationInfo.Create(animationInfo.AnimationName, animationInfo.Sprites.Length);
						var spriteSheet = AssetDatabase.GetAssetPath(targetTexture);
						var targetSprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();

						for (int i = 0; i < animationInfo.Sprites.Length; i++)
						{
							var spriteInfo = animationInfo.Sprites[i];
							var splits = spriteInfo.Sprite.name.Split('_');

							if (splits.Length > 0)
							{
								var spriteNum = Convert.ToInt32(splits[^1]);
								var targetSprite = targetSprites[spriteNum];
								var info = new SpriteInfo();
								info.Sprite = targetSprite;
								info.FrameDuration = spriteInfo.FrameDuration;
								targetAnimationInfo.Sprites[i] = info;
							}
						}

						targetData.Animations.Add(targetAnimationInfo);
					}
				}

				var path = outputPath;

				if (path[^1] == '/')
				{
					path = outputPath.Substring(0, outputPath.Length - 1);
				}

				foreach (var animationData in dataMap.Values)
				{
					AssetDatabase.CreateAsset(animationData, $"{path}/{animationData.name}.asset");
				}

				AssetDatabase.SaveAssets();
			}
		}

		[MenuItem("RAMG Project/JSAnim2D/Sprite Animation Clone")]
		public static void ShowEditor()
		{
			var window = GetWindow<JSSpriteAnimationCloneEditor>();
			window.titleContent = new GUIContent("Sprite Animation Clone");
			window.Show();
		}
	}
}
