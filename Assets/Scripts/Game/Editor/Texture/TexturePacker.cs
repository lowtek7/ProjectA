using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Editor.Texture
{
	[CreateAssetMenu(menuName = "TexturePacking/TexturePacker", fileName = "NewTexturePacker")]
	public class TexturePacker : ScriptableObject
	{
		public List<Texture2D> textures;
		public string filePath = "Assets/GameAsset";
		public string fileName = "Texture";
	}
}
