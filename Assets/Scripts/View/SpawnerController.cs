using System;
using BlitzEcs;
using Game.Ecs.Component;
using Game.Service;
using UnityEngine;
using UnityEngine.Pool;
using View.Behaviours;

namespace View
{
	/// <summary>
	/// 얘가 책임지고 게임오브젝트를 생성해서 ECS 월드에 있는 엔티티와 결합 시킴
	/// 풀링을 활용하는게 좋음 (Object Pool을 쓰는게 제일 좋다)
	/// </summary>
	public class SpawnerController : MonoBehaviour
	{
		// 얘가 활동을 시작한다
		public void Start()
		{
			Spawner.OnSpawnEvent += OnSpawn;
		}

		public void OnDestroy()
		{
			Spawner.OnSpawnEvent -= OnSpawn;
		}

		public void OnSpawn(Entity entity)
		{
			// 바인딩 해주면 된다 여기서.
			var go = new GameObject();

			if (entity.Has<PlayerComponent>())
			{
				// 셋업
				go.AddComponent<PlayerBehaviour>().SetupEntity(entity);
			}
		}
	}
}
