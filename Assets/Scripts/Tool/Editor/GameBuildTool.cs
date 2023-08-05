using Core.Virtual;
using Service.SaveLoad;
using Tool.Editor.Stage;
using Tool.Stage;
using UnityEditor;
using UnityEngine;

namespace Tool.Editor
{
	public class GameBuildTool : EditorWindow
	{
		/// <summary>
		/// 모든 스테이지를 빌드
		/// </summary>
		private void BuildAllStage()
		{
			// 스테이지 프리팹의 경로는 다음과 같음.
			// Assets/GameAsset/Prefab/StagePrefab
			string[] prefabs = AssetDatabase.FindAssets("t:Prefab", new [] { BuildConstants.StagePrefabPath });

			if (prefabs is { Length: > 0 })
			{
				var virtualWorld = new VirtualWorld();

				foreach (var prefabGuid in prefabs)
				{
					string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
					var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

					if (gameObject != null)
					{
						if (gameObject.TryGetComponent<StageSetting>(out var stageSetting))
						{
							StageBuilder.Build(stageSetting, virtualWorld);
						}
					}
				}

				SaveLoadService.SaveWorld(SaveLoadConstants.WorldDataPath, virtualWorld);
			}
		}

		private void OnGUI()
		{
			// 모든 데이터를 빌드하는 명령
			if (GUILayout.Button("Build All"))
			{
				BuildAllStage();
			}
		}

		[MenuItem("RAMG Project/Build Tool")]
		private static void ShowTool()
		{
			var window = GetWindow<GameBuildTool>();
			window.titleContent = new GUIContent("Game Build Tool");
			window.Show();
		}
	}
}
