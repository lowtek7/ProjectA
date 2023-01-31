using System;
using BlitzEcs;

namespace Game.Service
{
	public static class Spawner
	{
		/// <summary>
		/// 델리게이트 사용
		/// </summary>
		public static Action<Entity> OnSpawnEvent { get; set; }

		public static void Spawn()
		{
			// blah- 만들어짐 
			Entity result = new Entity();
			OnSpawnEvent?.Invoke(result);
		}
	}
}
