﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityService.Texture
{
	public class PackedTextureUvHolder : ScriptableObject
	{
		[Serializable]
		public struct PackedTextureUv
		{
			public int startX;

			public int startY;

			public Texture2D originTexture;
		}

		public List<PackedTextureUv> uvs = new();

		public int textureSize = 1 << 8;

		private Dictionary<string, PackedTextureUv> _nameToUvs;
		public Dictionary<string, PackedTextureUv> NameToUvs => _nameToUvs ??= uvs.ToDictionary(uv => uv.originTexture.name);
	}
}