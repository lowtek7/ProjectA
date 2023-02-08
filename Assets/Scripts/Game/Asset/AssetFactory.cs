using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utility;
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
		string Name { get; }
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
		private readonly Dictionary<string, IAssetModule> modules = new Dictionary<string, IAssetModule>();

		public void Init()
		{
			modules.Clear();
			var types = TypeUtility.GetTypesWithInterface(typeof(IAssetModule));

			foreach (var type in types)
			{
				if (Activator.CreateInstance(type) is IAssetModule assetModule)
				{
					modules.Add(assetModule.Name, assetModule);
				}
			}
		}

		public IEnumerator LoadAll()
		{
			foreach (var keyValuePair in modules)
			{
				yield return keyValuePair.Value.LoadAll();
			}
		}

		public bool TryGetAssetReader(string moduleName, out IAssetReader assetReader)
		{
			assetReader = null;

			if (modules.TryGetValue(moduleName, out var module))
			{
				assetReader = module;
				return true;
			}
			
			return false;
		}

		public bool TryGetAsset<CT>(string moduleName, string assetKey, out CT result) where CT : UnityEngine.Object
		{
			result = null;

			if (modules.TryGetValue(moduleName, out var module) && module.TryGet(assetKey, out result))
			{
				return true;
			}

			return false;
		}
	}
}
