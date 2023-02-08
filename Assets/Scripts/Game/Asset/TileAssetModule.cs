using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace Game.Asset
{
	public class TileAssetModule : IAssetModule
	{
		private readonly Dictionary<string, TileBase> tiles = new ();
		private readonly Dictionary<string, List<TileBase>> pathToTiles = new ();

		private bool isLoading = false;
		public IEnumerator LoadAll()
		{
			isLoading = true;
			tiles.Clear();
			Addressables.LoadAssetsAsync<TileBase>("Tile", null).Completed += OnLoadCompleted;

			while (isLoading)
			{
				yield return null;
			}
		}

		public bool TryGet<CT>(string key, out CT result) where CT : Object
		{
			result = null;
			if (tiles.TryGetValue(key, out var tile))
			{
				result = tile as CT;
				return true;
			}

			return false;
		}

		private void OnLoadCompleted(AsyncOperationHandle<IList<TileBase>> tileList)
		{
			if (tileList.Result != null)
			{
				foreach (var tile in tileList.Result)
				{
					var fullPath = AssetDatabase.GetAssetPath(tile);

					var key = fullPath.Replace(".asset", string.Empty)
						.Replace("Assets/GameAsset/Tile/", string.Empty);

					Debug.Log($"Loading tile [{key}]");

					var path = key.Split('/')[0];

					tiles.Add(key, tile);

					// 추후 여러 챕터가 생길 때를 위해 Path별로 타일을 나누도록 함.
					if (!pathToTiles.TryGetValue(path, out var tilesByPath))
					{
						tilesByPath = new List<TileBase>();

						pathToTiles.Add(path, tilesByPath);
					}

					tilesByPath.Add(tile);
				}

				GC.Collect();

				isLoading = false;
			}
		}

		public IReadOnlyList<TileBase> GetInPath(string path)
		{
			return pathToTiles[path];
		}
	}
}
