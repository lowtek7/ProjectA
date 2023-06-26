using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Editor.Texture
{
	[Serializable]
	public class BlockSpec
	{
		public string name = "new_block";
		public Texture2D[] textures = new Texture2D[6]
		{
			null,
			null,
			null,
			null,
			null,
			null,
		};
	}

	[CreateAssetMenu(menuName = "BlockData/Texture", fileName = "NewBlockSpecHolder")]
	public class BlockSpecHolder : ScriptableObject
	{
		public List<BlockSpec> blockSpecs = new();

		private Dictionary<string, BlockSpec> _nameToSpec;

		public Dictionary<string, BlockSpec> NameToSpec => _nameToSpec ??= blockSpecs.ToDictionary(spec => spec.name);
	}
}
