using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Game.Ecs.System
{
	/// <summary>
	/// 디버그 메세지가 있으면 출력하는 테스트용 시스템
	/// </summary>
	public class TestDebugSystem : ISystem
	{
		public int Order => 1;

		private Query<DebugMessageComponent> query;

		public void Init(BlitzEcs.World world)
		{
			query = new Query<DebugMessageComponent>(world);
		}

		public void Update(float deltaTime)
		{
			query.Fetch();

			foreach (var entity in query)
			{
				ref var debugMessage = ref entity.Get<DebugMessageComponent>();

				if (!string.IsNullOrEmpty(debugMessage.message))
				{
					Debug.LogWarning(debugMessage.message);
					debugMessage.message = string.Empty;
				}
			}
		}
	}
}
