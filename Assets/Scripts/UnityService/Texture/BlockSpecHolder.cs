using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityService.Texture
{
	[Serializable]
	public class BlockSpec
	{
		public string name = "new_block";
		public Texture2D[] textures = new Texture2D[6];
	}

	[CreateAssetMenu(menuName = "BlockData/Texture", fileName = "NewBlockSpecHolder")]
	public class BlockSpecHolder : ScriptableObject
	{
		public List<BlockSpec> blockSpecs = new();
	}
}
