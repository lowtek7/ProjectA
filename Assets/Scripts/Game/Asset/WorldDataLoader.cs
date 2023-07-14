using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Asset
{
	/// <summary>
	/// 일단 임시로 월드 데이터 에셋 가져오는 로더 구현.
	/// </summary>
	public static class WorldDataAssetLoader
	{
		private static readonly Dictionary<string, TextAsset> textAssets = new Dictionary<string, TextAsset>();

		private static bool _isLoading = false;

		public static TextAsset WorldDataAsset => textAssets.FirstOrDefault().Value;

		public static IEnumerator LoadAll()
		{
			_isLoading = true;
			textAssets.Clear();
			Addressables.LoadAssetsAsync<TextAsset>("WorldData", null).Completed += OnLoadCompleted;

			while (_isLoading)
			{
				yield return null;
			}
		}

		private static void OnLoadCompleted(AsyncOperationHandle<IList<TextAsset>> assets)
		{
			if (assets.Result != null)
			{
				foreach (var textFile in assets.Result)
				{
					Debug.Log($"Loading Texture [{textFile.name}]");
					textAssets.TryAdd(textFile.name, textFile);
				}

				GC.Collect();

				_isLoading = false;
			}
		}
	}
}
