using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Game.Asset
{
	public class ScriptableAssetModule : IAssetModule
	{
		private readonly Dictionary<string, ScriptableObject> _scriptableObjects = new();

		private bool _isLoading = false;

		public void Init(AssetFactory assetFactory)
		{
		}

		public IEnumerator LoadAll()
		{
			_isLoading = true;
			_scriptableObjects.Clear();
			Addressables.LoadAssetsAsync<ScriptableObject>("ScriptableObject", null).Completed += OnLoadCompleted;

			while (_isLoading)
			{
				yield return null;
			}
		}

		private void OnLoadCompleted(AsyncOperationHandle<IList<ScriptableObject>> scriptableObjects)
		{
			if (scriptableObjects.Result == null)
			{
				return;
			}

			foreach (var scriptableObject in scriptableObjects.Result)
			{
				{
					Debug.Log($"Loading ScriptableObject [{scriptableObject.name}]");
					_scriptableObjects.Add(scriptableObject.name, scriptableObject);
				}

				GC.Collect();

				_isLoading = false;
			}
		}

		public bool TryGet<CT>(string key, out CT result) where CT : Object
		{
			if (_scriptableObjects.TryGetValue(key, out var scriptableObject))
			{
				result = scriptableObject as CT;
				return true;
			}

			result = null;
			return false;
		}
	}
}
