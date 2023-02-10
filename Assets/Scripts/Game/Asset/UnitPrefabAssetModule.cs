using System;
using System.Collections;
using System.Collections.Generic;
using Game.Unit;
using Library.JSPool;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Game.Asset
{
	/// <summary>
	/// UnitPrefab들이 보관된 에셋 모듈
	/// </summary>
	public class UnitPrefabAssetModule : IAssetModule
	{
		private readonly struct UnitPrefabData
		{
			public UnitTemplate UnitTemplate { get; }
			public GameObject GameObject => UnitTemplate.gameObject;
			private bool IncludedPoolItem { get; }
			private PoolItem PoolItem { get; }

			/// <summary>
			/// Pool 지원을 받는것이 가능한 것은 PoolItem에 대해 가져 올 수 있다.
			/// </summary>
			/// <param name="poolItem"></param>
			/// <returns></returns>
			public bool TryGetPoolItem(out PoolItem poolItem)
			{
				poolItem = PoolItem;
				return IncludedPoolItem;
			}

			public UnitPrefabData(UnitTemplate unitTemplate)
			{
				UnitTemplate = unitTemplate;

				IncludedPoolItem = false;
				PoolItem = null;
				if (UnitTemplate.TryGetComponent<PoolItem>(out var poolItem))
				{
					IncludedPoolItem = true;
					PoolItem = poolItem;
				}
			}
		}

		/// <summary>
		/// Key = SourceGUID
		/// Value = Prefab Data
		/// </summary>
		private readonly Dictionary<Guid, UnitPrefabData> unitPrefabs = new Dictionary<Guid, UnitPrefabData>();

		private bool isLoading = false;

		private AssetFactory _assetFactory;

		public void Init(AssetFactory assetFactory)
		{
			_assetFactory = assetFactory;
		}

		public IEnumerator LoadAll()
		{
			isLoading = true;
			unitPrefabs.Clear();
			Addressables.LoadAssetsAsync<GameObject>("EntityPreset", null).Completed += OnLoadCompleted;

			while (isLoading)
			{
				yield return null;
			}
		}

		public bool TryGet<CT>(string key, out CT result) where CT : Object
		{
			result = null;
			if (Guid.TryParse(key, out var guid))
			{
				if (unitPrefabs.TryGetValue(guid, out var unitPrefabData))
				{
					result = unitPrefabData.UnitTemplate as CT;
					return true;
				}
			}

			return false;
		}
		
		private void OnLoadCompleted(AsyncOperationHandle<IList<GameObject>> archetypeList)
		{
			if (archetypeList.Result != null)
			{
				foreach (var go in archetypeList.Result)
				{
					if (go.TryGetComponent<UnitTemplate>(out var unitTemplate))
					{
						Debug.Log($"Loading UnitPrefab [{go.name}]");
						unitPrefabs.Add(unitTemplate.SourceGuid, new UnitPrefabData(unitTemplate));
					}
					else
					{
						Debug.LogError($"Load Failed! UnitPrefab [{go.name}] is not a UnitTemplate!");
					}
					
				}

				GC.Collect();

				isLoading = false;
			}
		}

		public bool TrySpawn(Guid sourceGuid, Transform parent, out UnitTemplate unitTemplate)
		{
			unitTemplate = null;
			if (unitPrefabs.TryGetValue(sourceGuid, out var unitPrefabData))
			{
				if (unitPrefabData.TryGetPoolItem(out var poolItem))
				{
					var result = _assetFactory.PoolManager.Spawn(poolItem.ItemGuid, parent);
					unitTemplate = result.GetComponent<UnitTemplate>();
				}
				else
				{
					var result = GameObject.Instantiate(unitPrefabData.GameObject, Vector3.zero, Quaternion.identity, parent);
					unitTemplate = result.GetComponent<UnitTemplate>();
				}

				return true;
			}

			Debug.LogError($"Spawn Failed. {sourceGuid} is not a Unit Prefab!");
			return false;
		}

		public bool TrySpawn(Guid sourceGuid, out UnitTemplate unitTemplate) =>
			TrySpawn(sourceGuid, null, out unitTemplate);

		/// <summary>
		/// 주의) Despawn에 들어오기전에 무조건 Disconnect를 호출한 상태로 들어와야 한다!
		/// </summary>
		/// <param name="unitTemplate"></param>
		public void Despawn(UnitTemplate unitTemplate)
		{
			var sourceGuid = unitTemplate.SourceGuid;

			if (unitPrefabs.TryGetValue(sourceGuid, out var unitPrefabData))
			{
				if (unitPrefabData.TryGetPoolItem(out var poolItem))
				{
					_assetFactory.PoolManager.Despawn(unitTemplate.gameObject);
				}
				else
				{
					// Pool 지원을 받지 않는 객체는 직접 지워줘야 한다.
					GameObject.Destroy(unitTemplate.gameObject);
				}
			}
		}
	}
}
