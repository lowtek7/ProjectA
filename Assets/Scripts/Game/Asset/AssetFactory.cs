using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utility;
using Game.Asset;
using Library.JSPool;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game
{
	public interface IAssetReader
	{
		bool TryGet<CT>(string key, out CT result) where CT : UnityEngine.Object;
	}

	public interface IAssetModule : IAssetReader
	{
		void Init(AssetFactory assetFactory);
		IEnumerator LoadAll();
	}

	/// <summary>
	/// 에셋 팩토리의 수명을 인게임 씬을 따라가기 위해서 MonoBehaviour
	/// 에셋 팩토리의 인스턴스는 게임 로더 측에서 가지고 있음.
	/// 현재는 에셋의 종류가 추가되면 팩토리에 하나씩 다 증가하게 되는데 모듈화하는 방법을 생각중
	/// 예를 들면 archetype들만 불러오는 archetype module을 에셋팩토리에 넣어서 사용하는 방식
	/// </summary>
	public class AssetFactory : MonoBehaviour
	{
		private static AssetFactory instance;

		public static AssetFactory Instance => instance;

		private readonly Dictionary<Type, IAssetModule> modules = new Dictionary<Type, IAssetModule>();

		private PoolManager _poolManager;

		public PoolManager PoolManager => _poolManager;

		public void Init(PoolManager poolManager)
		{
			instance = this;
			_poolManager = poolManager;

			modules.Clear();
			var types = TypeUtility.GetTypesWithInterface(typeof(IAssetModule));

			foreach (var type in types)
			{
				if (Activator.CreateInstance(type) is IAssetModule assetModule)
				{
					modules.Add(assetModule.GetType(), assetModule);
				}
			}

			foreach (var keyValues in modules)
			{
				keyValues.Value.Init(this);
			}
		}

		public IEnumerator LoadAll()
		{
			foreach (var keyValuePair in modules)
			{
				yield return keyValuePair.Value.LoadAll();
			}
		}

		public bool TryGetAssetReader<CT>(out IAssetReader assetReader) where CT : IAssetReader
		{
			assetReader = null;

			if (modules.TryGetValue(typeof(CT), out var module))
			{
				assetReader = module;
				return true;
			}

			return false;
		}

		public bool TryGetAsset<CT, CU>(string assetKey, out CU result) where CT : IAssetReader where CU : UnityEngine.Object
		{
			result = null;

			if (modules.TryGetValue(typeof(CT), out var module) && module.TryGet(assetKey, out result))
			{
				return true;
			}

			return false;
		}
	}
}
