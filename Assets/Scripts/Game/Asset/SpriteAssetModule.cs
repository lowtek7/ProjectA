using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Game.Asset
{
	public class SpriteAssetModule : IAssetModule
	{
		private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

		private bool isLoading = false;
		public void Init(AssetFactory assetFactory)
		{
		}

		public IEnumerator LoadAll()
		{
			isLoading = true;
			sprites.Clear();
			Addressables.LoadAssetsAsync<Sprite>("Sprite", null).Completed += OnLoadCompleted;

			while (isLoading)
			{
				yield return null;
			}
		}

		public bool TryGet<CT>(string key, out CT result) where CT : Object
		{
			result = null;
			if (sprites.TryGetValue(key, out var sprite))
			{
				result = sprite as CT;
				return true;
			}

			return false;
		}

		private void OnLoadCompleted(AsyncOperationHandle<IList<Sprite>> spriteList)
		{
			if (spriteList.Result != null)
			{
				foreach (var sprite in spriteList.Result)
				{
					Debug.Log($"Loading Sprite [{sprite.name}]");
					sprites.Add(sprite.name, sprite);
				}

				GC.Collect();

				isLoading = false;
			}
		}
	}
}
