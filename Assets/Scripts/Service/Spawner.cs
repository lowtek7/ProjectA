using System;
using BlitzEcs;

namespace Game.Service
{	
	/// <summary>
	/// 유니티에서 ECS 시스템의 Entity를 만들고, 제거하는 등 관리하는 함수.
	/// </summary>
	public static class Spawner
	{
		public static Action<Entity> OnSpawnEvent { get; set; }

		public static Action<Entity> OnDespawnEvent { get; set; }

		/// <summary>
		/// Entity를 만들고, 이벤트를 통해 ECS 시스템에 전달한다.
		/// </summary>
		public static void Spawn()
		{
			Entity result = new Entity();
			OnSpawnEvent?.Invoke(result);
		}

		//TODO: 유니티에서 제거할 ECS Entity를 인자로 넣을 방법이 있나?
		// 아니면 게임 오브젝트를 인자로 넣으면 내부에서 Entity를 찾아주어야 하나?
		public static void Despawn(Entity despawnEntity)
        {
			OnDespawnEvent?.Invoke(despawnEntity);
        }
	}
}
