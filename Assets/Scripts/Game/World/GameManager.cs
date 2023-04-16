using System;
using System.Collections.Generic;
using System.Linq;
using BlitzEcs;
using Core.Unity;
using Core.Utility;
using Game.Ecs.Component;
using Library.JSPool;
using Service;
using Service.SaveLoad;
using UnityEngine;

namespace Game.World
{
	/// <summary>
	/// 임시용 게임 월드
	/// </summary>
	public class GameManager : IServiceManagerCallback, IDisposable
	{
		private PoolManager poolManager;
		private GameObject camera;

		private BlitzEcs.World world;

		private readonly List<ISystem> systems = new List<ISystem>();

		private Query<PlayerComponent, MovementComponent> playerQuery;

		/// <summary>
		/// 원래는 에셋 팩토리에 직접 접근하는 일이 없어야 함
		/// 현재 엔티티 스폰을 위해서 임시적으로 이렇게 구성했음
		/// 추후에 spawner를 따로 만들거나 따로 그러한 작업을 위한 서비스를 구축해야함
		/// 카메라 또한 카메라 엔티티를 따로 만들어서 컨트롤 해야함
		/// </summary>
		/// <param name="gameLoader"></param>
		public void Init(GameLoader gameLoader)
		{
			poolManager = gameLoader.PoolManager;
			poolManager.Init();

			// ecs 월드를 생성하자.
			world = new BlitzEcs.World();

			Dictionary<int, List<ISystem>> systemOrders = new Dictionary<int, List<ISystem>>();

			// 리플렉션을 이용해서 ISystem의 구현체들을 모은다.
			foreach (var systemType in TypeUtility.GetTypesWithInterface(typeof(ISystem)))
			{
				if (Activator.CreateInstance(systemType) is ISystem system)
				{
					var order = (int)system.Order;
					if (!systemOrders.TryGetValue(order, out var innerSystems))
					{
						innerSystems = new List<ISystem>();
						systemOrders[order] = innerSystems;
					}
					innerSystems.Add(system);
				}
			}

			var systemLists = systemOrders.OrderBy(pair => pair.Key).Select(x => x.Value).ToArray();

			foreach (var systemList in systemLists)
			{
				foreach (var system in systemList)
				{
					// order에 기반한 스테이지 초기화 작업
					system.Init(world);
					systems.Add(system);
				}
			}

			foreach (var service in ServiceManager.Services)
			{
				service.Init(world);
			}

			var virtualWorld = SaveLoadService.LoadWorld(SaveLoadConstants.WorldDataPath);
			virtualWorld.Realize(world);

			playerQuery = new Query<PlayerComponent, MovementComponent>(world);
			ServiceManager.AddCallback(this);
		}

		/// <summary>
		/// 추후에 유니티 시간이 아닌 내부 월드 타임을 따로 두어서 사용 할 것.
		/// </summary>
		public void Update()
		{
			var dt = Time.deltaTime;

			foreach (var system in systems)
			{
				system.Update(dt);
			}
		}

		public void LateUpdate()
		{
			var dt = Time.deltaTime;

			foreach (var system in systems)
			{
				system.Update(dt);
			}
		}

		public void OnActivateService(IGameService service)
		{
			if (world != null)
			{
				service.Init(world);
			}
		}

		public void OnDeactivateService(IGameService service)
		{
		}

		public void Dispose()
		{
			world = null;
			ServiceManager.RemoveCallback(this);
		}
	}
}
