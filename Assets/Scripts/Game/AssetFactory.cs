﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game
{
	/// <summary>
	/// 에셋 팩토리의 수명을 인게임 씬을 따라가기 위해서 MonoBehaviour
	/// 에셋 팩토리의 인스턴스는 게임 로더 측에서 가지고 있음.
	/// 현재는 에셋의 종류가 추가되면 팩토리에 하나씩 다 증가하게 되는데 모듈화하는 방법을 생각중
	/// 예를 들면 archetype들만 불러오는 archetype module을 에셋팩토리에 넣어서 사용하는 방식
	/// </summary>
	public class AssetFactory : MonoBehaviour
	{
		private readonly Dictionary<string, GameObject> archetypes = new Dictionary<string, GameObject>();

		/// <summary>
		/// 모든 아키타입 리소스를 불러오는 함수
		/// </summary>
		public void LoadAllArchetype()
		{
			archetypes.Clear();
			Addressables.LoadAssetsAsync<GameObject>("Archetype", null).Completed += OnArchetypeLoadCompleted;
		}

		public bool TryGetArchetype(string key, out GameObject go) => archetypes.TryGetValue(key, out go);

		private void OnArchetypeLoadCompleted(AsyncOperationHandle<IList<GameObject>> archetypeList)
		{
			if (archetypeList.Result != null)
			{
				foreach (var go in archetypeList.Result)
				{
					Debug.Log($"Loading Archetype [{go.name}]");
					archetypes.Add(gameObject.name, go);
				}
				
				GC.Collect();
			}
		}
	}
}