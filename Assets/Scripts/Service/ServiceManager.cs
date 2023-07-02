using System;
using System.Collections.Generic;
using Core.Unity;

namespace Service
{
	public interface IServiceManagerCallback
	{
		void OnActivateService(IGameService service);
		void OnDeactivateService(IGameService service);
	}

	public static class ServiceManager
	{
		private static Dictionary<Type, IGameService> serviceMap = new Dictionary<Type, IGameService>();

		private static List<IServiceManagerCallback> callbacks = new List<IServiceManagerCallback>();

		public static IEnumerable<IGameService> Services => serviceMap.Values;

		public static bool TryGetService<CT>(out CT service) where CT : IGameService
		{
			service = default;

			if (serviceMap.TryGetValue(typeof(CT), out var result))
			{
				service = (CT)result;
				return true;
			}

			return false;
		}

		/// <summary>
		/// 서비스를 셋팅하는 함수. 매우 조심하게 사용해야한다.
		/// 서비스를 셋팅할때는 서비스의 타입을 무조건 IGameService를 상속받은 인터페이스 서비스로 등록해야 한다.
		/// 예를 들어 GamepadInputService는 IGameInputService의 구현체라면
		/// ServiceType에는 IGameInputService를 넣어줘야한다.
		/// 그리고 IGameInputService는 IGameService를 상속 받고 있어야 한다.
		/// </summary>
		/// <param name="serviceInterfaceType">해당 서비스의 인터페이스 타입</param>
		/// <param name="service">서비스 객체</param>
		/// <typeparam name="CT"></typeparam>
		public static void SetService<CT>(Type serviceInterfaceType, CT service) where CT : class, IGameService
		{
			if (service == null)
			{
				return;
			}

			{
				if (serviceMap.TryGetValue(serviceInterfaceType, out var original))
				{
					{
						if (original is IGameServiceCallback callback)
						{
							callback.OnDeactivate();
						}
					}

					foreach (var callback in callbacks)
					{
						callback.OnDeactivateService(service);
					}
				}
			}

			serviceMap[serviceInterfaceType] = service;

			{
				if (service is IGameServiceCallback callback)
				{
					callback.OnActivate();
				}
			}

			foreach (var callback in callbacks)
			{
				callback.OnActivateService(service);
			}
		}

		/// <summary>
		/// 사용 할일이 있을까?... 일단 구현 해둔다.
		/// </summary>
		/// <param name="serviceInterfaceType"></param>
		/// <typeparam name="CT"></typeparam>
		public static void ClearService(Type serviceInterfaceType)
		{
			if (serviceMap.TryGetValue(serviceInterfaceType, out var service))
			{
				{
					if (service is IGameServiceCallback callback)
					{
						callback.OnDeactivate();
					}
				}

				serviceMap.Remove(serviceInterfaceType);

				foreach (var callback in callbacks)
				{
					callback.OnDeactivateService(service);
				}
			}
		}

		public static void AddCallback(IServiceManagerCallback callback)
		{
			callbacks.Add(callback);
		}

		public static void RemoveCallback(IServiceManagerCallback callback)
		{
			var index = callbacks.FindIndex(x => x == callback);

			if (index >= 0)
			{
				callbacks.RemoveAt(index);
			}
		}

		public static void Update(float deltaTime)
		{
			foreach (var keyValuePair in serviceMap)
			{
				if (keyValuePair.Value is IUpdate update)
				{
					update.UpdateProcess(deltaTime);
				}
			}
		}

		public static void Dispose()
		{
			foreach (var service in serviceMap.Values)
			{
				{
					if (service is IGameServiceCallback callback)
					{
						callback.OnDeactivate();
					}
				}

				foreach (var callback in callbacks)
				{
					callback.OnDeactivateService(service);
				}
			}

			serviceMap.Clear();
		}
	}
}
