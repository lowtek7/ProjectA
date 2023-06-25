using System.Collections.Generic;
using UnityEngine;

namespace Game.Editor.Texture
{
	[CreateAssetMenu(menuName = "TexturePacking/TexturePacker", fileName = "NewTexturePacker")]
	public class TexturePacker : ScriptableObject
	{
		public List<Texture2D> textures;
		public string filePath = "Assets/GameAsset/Textures/Packed";
		public string fileName = "PackedTexture";
		public int maxPixelSize = 1 << 8;

		public string uvDataPath = "Assets/GameAsset/ScriptableObject/Texture/Uvs";
		public string uvDataName = "PackedTextureUvs";
	}
}
