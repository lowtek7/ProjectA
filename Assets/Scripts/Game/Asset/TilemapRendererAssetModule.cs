﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Game.Asset
{
	public class TilemapRendererAssetModule : IAssetModule
	{
		private readonly Dictionary<string, GameObject> entityPresets = new Dictionary<string, GameObject>();

		private bool isLoading = false;

		public void Init(AssetFactory assetFactory)
		{
		}

		public IEnumerator LoadAll()
		{
			isLoading = true;
			entityPresets.Clear();
			Addressables.LoadAssetsAsync<GameObject>("TileRenderer", null).Completed += OnLoadCompleted;

			while (isLoading)
			{
				yield return null;
			}
		}

		public bool TryGet<CT>(string key, out CT result) where CT : Object
		{
			result = null;
			if (entityPresets.TryGetValue(key, out var go))
			{
				result = go as CT;
				return true;
			}

			return false;
		}

		private void OnLoadCompleted(AsyncOperationHandle<IList<GameObject>> archetypeList)
		{
			if (archetypeList.Result != null)
			{
				foreach (var go in archetypeList.Result)
				{
					Debug.Log($"Loading Tilemap Renderer [{go.name}]");
					entityPresets.Add(go.name, go);
				}

				GC.Collect();

				isLoading = false;
			}
		}
	}
}