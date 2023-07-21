using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Service;
using Service.Network;

namespace Game.Ecs.System
{
	public class NetworkEntitySystem : ISystem
	{
		private Query<PlayerComponent> playerQuery;

		public void Init(BlitzEcs.World world)
		{
			playerQuery = new Query<PlayerComponent>(world);
		}

		public void Update(float deltaTime)
		{
			if (ServiceManager.TryGetService(out INetClientService clientService))
			{
				foreach (var entity in playerQuery)
				{
					var playerComponent = entity.Get<PlayerComponent>();

					// 로컬 소유의 플레이어는 내 마음대로 컨트롤 할 수 있다.
					if (playerComponent.PlayerType == PlayerType.Local)
					{
						// net id가 없으면 생성해줘야한다
						if (!entity.Has<NetIdComponent>())
						{
							entity.Add<NetIdComponent>();
						}

						ref var netIdComponent = ref entity.Get<NetIdComponent>();

						netIdComponent.NetId = clientService.NetId;
					}
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
