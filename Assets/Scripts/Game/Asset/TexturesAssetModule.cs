using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Game.Asset
{
	public class TexturesAssetModule : IAssetModule
	{
		private readonly Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

		private bool isLoading = false;

		public bool TryGet<CT>(string key, out CT result) where CT : Object
		{
			result = null;
			if (textures.TryGetValue(key, out var texture))
			{
				result = texture as CT;
				return true;
			}

			return false;
		}

		public void Init(AssetFactory assetFactory)
		{
		}

		public IEnumerator LoadAll()
		{
			isLoading = true;
			textures.Clear();
			Addressables.LoadAssetsAsync<Texture>("Textures", null).Completed += OnLoadCompleted;

			while (isLoading)
			{
				yield return null;
			}
		}

		private void OnLoadCompleted(AsyncOperationHandle<IList<Texture>> textureList)
		{
			if (textureList.Result != null)
			{
				foreach (var texture in textureList.Result)
				{
					Debug.Log($"Loading Texture [{texture.name}]");
					textures.TryAdd(texture.name, texture);
				}

				GC.Collect();

				isLoading = false;
			}
		}
	}
}
