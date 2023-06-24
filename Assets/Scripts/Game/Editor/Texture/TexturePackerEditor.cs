using System;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Device.Application;
using IO = System.IO;

namespace Game.Editor.Texture
{
	[CustomEditor(typeof(TexturePacker))]
	public class TexturePackerEditor : UnityEditor.Editor
	{
		private TexturePacker _packer;

		private void OnEnable()
		{
			_packer = target as TexturePacker;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (Application.isPlaying)
				return;

			if (GUILayout.Button("Generate"))
			{
				GenerateTexture();
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void GenerateTexture()
		{
			var pixelSize = 1 << 8;

			var newTexture = new Texture2D(pixelSize, pixelSize);

			var drawPos = Vector2Int.zero;

			foreach (var texture in _packer.textures)
			{
				var width = texture.width;
				var height = texture.height;

				if (drawPos.x + width > newTexture.width)
				{
					// FIXME : 빈 공간을 찾아 떠나야 함
					drawPos = new Vector2Int(0, drawPos.y + height);
				}

				// TODO : 여기서 UV 정보도 같이 저장

				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						var pixel = texture.GetPixel(x, y);

						newTexture.SetPixel(drawPos.x + x, drawPos.y + y, pixel);
					}
				}

				drawPos.x += width;
			}

			newTexture.Apply();

			#region FileIO

			var dirPath = $"{_packer.filePath}";
			var fileName = $"{dirPath}/{_packer.fileName}.png";

			if (!IO.Directory.Exists(dirPath))
			{
				IO.Directory.CreateDirectory(dirPath);
			}

			var pngBytes = newTexture.EncodeToPNG();

			IO.File.WriteAllBytes(fileName, pngBytes);

			AssetDatabase.ImportAsset(fileName, ImportAssetOptions.ForceUpdate);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			#endregion

			var newTextureAsset = AssetDatabase.LoadAssetAtPath(fileName, typeof(Texture2D)) as Texture2D;

			if (AssetDatabase.IsMainAsset(newTextureAsset))
			{
				var originImporter = AssetImporter.GetAtPath(
					AssetDatabase.GetAssetPath(_packer.textures[0])) as UnityEditor.TextureImporter;

				var originSetting = new TextureImporterSettings();
				originImporter.ReadTextureSettings(originSetting);

				var originPlatformSetting = originImporter.GetDefaultPlatformTextureSettings();

				var newImporter = AssetImporter.GetAtPath(fileName) as UnityEditor.TextureImporter;
				var newSetting = new TextureImporterSettings();
				var newPlatformSetting = new TextureImporterPlatformSettings();

				originSetting.CopyTo(newSetting);
				originPlatformSetting.CopyTo(newPlatformSetting);

				newSetting.readable = false;
				newPlatformSetting.maxTextureSize = pixelSize;

				newImporter.SetTextureSettings(newSetting);
				newImporter.SetPlatformTextureSettings(newPlatformSetting);

				if (AssetDatabase.WriteImportSettingsIfDirty(newImporter.assetPath))
				{
					newImporter.SaveAndReimport();
				}
			}

			// TODO : UV 정보 Export
		}
	}
}
