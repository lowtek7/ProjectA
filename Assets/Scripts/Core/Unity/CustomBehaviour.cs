using System;
using BlitzEcs;
using UnityEngine;

namespace Core.Unity
{
	/// <summary>
	/// 해당 인터페이스를 구현하는 CustomBehaviour는 update call을 받게 된다.
	/// </summary>
	public interface IUpdate
	{
		void UpdateProcess(float deltaTime);
	}

	public class CustomBehaviour : MonoBehaviour
	{
		protected virtual void Awake()
		{
			if (CoreMachine.TryGetInstance(out var coreMachine))
			{
				if (this is IUpdate update)
				{
					coreMachine.RegisterUpdater(update);
				}
			}
		}

		protected virtual void OnDestroy()
		{
			if (CoreMachine.TryGetInstance(out var coreMachine))
			{
				if (this is IUpdate update)
				{
					coreMachine.UnregisterUpdater(update);
				}
			}
		}
	}
}
