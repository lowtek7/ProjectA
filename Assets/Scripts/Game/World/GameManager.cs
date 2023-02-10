using System;
using System.Collections.Generic;
using System.Linq;
using BlitzEcs;
using Core.Unity;
using Core.Utility;
using Game.Ecs.Component;
using Game.Service;
using Library.JSPool;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.World
{
	/// <summary>
	/// 임시용 게임 월드
	/// </summary>
	public class GameManager
	{
		private PoolManager poolManager;
		private GameObject camera;

		private Vector3 cameraDist = new Vector3(0, 0, -3);

		private float moveSpeed = 10;

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
					if (!systemOrders.TryGetValue(system.Order, out var systems))
					{
						systems = new List<ISystem>();
						systemOrders[system.Order] = systems;
					}
					systems.Add(system);
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

			// 서비스들 초기화 해주기 (임시적)
			foreach (var service in gameLoader.Services)
			{
				service.Init(world);
				Debug.Log($"Service[{service.GetType()}] Init Call");
			}

			// 일단 캐릭터들 스폰하는거 하드 코딩으로 작성해둠
			// 사용할 source guid : 9e6f16ba-3f5e-4d8a-8d42-16c8984134d4
			var sourceGuid = Guid.Parse("9e6f16ba-3f5e-4d8a-8d42-16c8984134d4");
			var spawnCommanders = new List<SpawnCommander>();

			// player부터 작성하기
			spawnCommanders.Add(Spawner.Spawn(world)
				.Add(new UnitComponent
				{
					SourceGuid = sourceGuid
				})
				.Add(new PlayerCameraComponent())
				.Add(new PlayerComponent())
				.Add(new TransformComponent
				{
					Position = Vector3.zero
				})
				.Add(new ZoneComponent
				{
					StageId = 0
				})
				.Add(new MovementComponent
				{
					MoveDir = Vector3.zero,
					MoveSpeed = 1
				}));

			// 다른 캐릭터들 그려주기
			for (int i = 0; i < 20; i++)
			{
				var stageId = Random.Range(0, 5);
				var pos = new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f), 0);
				spawnCommanders.Add(Spawner.Spawn(world)
					.Add(new UnitComponent
					{
						SourceGuid = sourceGuid
					})
					.Add(new TransformComponent
					{
						Position = pos
					})
					.Add(new ZoneComponent
					{
						StageId = stageId
					}));
			}

			foreach (var commander in spawnCommanders)
			{
				commander.Commit();
			}

			playerQuery = new Query<PlayerComponent, MovementComponent>(world);
		}

		/// <summary>
		/// 추후에 유니티 시간이 아닌 내부 월드 타임을 따로 두어서 사용 할 것.
		/// </summary>
		public void Update()
		{
			var dt = Time.deltaTime;

			float x = 0;
			float y = 0;

			if (Input.GetKey(KeyCode.W))
			{
				y += 1;
			}

			if (Input.GetKey(KeyCode.S))
			{
				y -= 1;
			}

			if (Input.GetKey(KeyCode.A))
			{
				x -= 1;
			}

			if (Input.GetKey(KeyCode.D))
			{
				x += 1;
			}

			playerQuery.Fetch();
			playerQuery.ForEach((ref PlayerComponent playerComponent,
				ref MovementComponent movementComponent) =>
			{
				movementComponent.MoveDir = new Vector3(x, y, 0);
			});

			foreach (var system in systems)
			{
				system.Update(dt);
			}
		}
	}
}
