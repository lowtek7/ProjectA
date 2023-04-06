using System;
using System.Reflection;
using Core.Utility;
using Service;
using UnityEngine;

namespace UnityService
{
	/// <summary>
	/// 이 Attribute를 달아줘야 자동화에 지원 받을 수 있다.
	/// </summary>
	public class UnityServiceAttribute : Attribute
	{
		public Type ServiceInterfaceType { get; }
		
		public UnityServiceAttribute(Type serviceInterfaceType)
		{
			ServiceInterfaceType = serviceInterfaceType;
		}
	}
	
	/// <summary>
	/// 유니티 서비스들을 자동으로 주입하는 인젝터.
	/// 추후 좋은 방법이 있다면 그러한 구조로 개선.
	/// </summary>
	public class UnityServiceAutoInjector : MonoBehaviour
	{
		private void Start()
		{
			// start 시점에서 서비스들을 수집하게 변경하자.
			var types = TypeUtility.GetTypesWithAttribute(typeof(UnityServiceAttribute));

			foreach (var type in types)
			{
				var unityServiceAttribute = type.GetCustomAttribute<UnityServiceAttribute>();
				if (unityServiceAttribute != null)
				{
					var interfaceType = unityServiceAttribute.ServiceInterfaceType;
					var result = FindObjectOfType(type);

					if (result is IGameService gameService)
					{
						ServiceManager.SetService(interfaceType, gameService);
					}
				}
			}
		}

		private void OnDestroy()
		{
			ServiceManager.Dispose();
		}
	}
}
