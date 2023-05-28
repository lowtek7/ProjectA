using System;
using System.Collections.Generic;
using System.Linq;
using Core.Unity;
using UnityEngine;

namespace Core
{
	/// <summary>
	/// Core 계층에서만 접근 할 수 있다.
	/// </summary>
	class CoreMachine : MonoBehaviour
	{
		private static bool isValidInstance = false;
		private static CoreMachine instance;

		/// <summary>
		/// 모노비헤이비어 널 체크는 퍼포먼스적으로 큰 작업이기 때문에 (라이더가 알려줌)
		/// 인스턴스의 널체크를 instanceFlag를 이용해 대신 한다
		/// </summary>
		/// <param name="coreMachine"></param>
		/// <returns></returns>
		public static bool TryGetInstance(out CoreMachine coreMachine)
		{
			coreMachine = instance;
			return isValidInstance;
		}

		private readonly HashSet<IUpdate> updaters = new HashSet<IUpdate>();

		private readonly List<IUpdate> updateBuffer = new List<IUpdate>();

		public void RegisterUpdater(IUpdate updater)
		{
			updaters.Add(updater);
		}

		public void UnregisterUpdater(IUpdate updater)
		{
			updaters.Remove(updater);
		}

		public void Update()
		{
			updateBuffer.Clear();
			updateBuffer.AddRange(updaters);
			
			var deltaTime = Time.deltaTime;

			foreach (var update in updateBuffer)
			{
				update.UpdateProcess(deltaTime);
			}
		}

		private void Awake()
		{
			instance = this;
			isValidInstance = true;
			updaters.Clear();
		}

		private void OnDestroy()
		{
			updaters.Clear();
			instance = null;
			isValidInstance = false;
		}
	}
}
