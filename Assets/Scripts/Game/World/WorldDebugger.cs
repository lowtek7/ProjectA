using BlitzEcs;
using Game.Ecs.Component;
using Game.Service;
using UnityEngine;

namespace Game.World
{
	public class WorldDebugger : MonoBehaviour, IGameService
	{
		private BlitzEcs.World selfWorld;

		public void Init(BlitzEcs.World world)
		{
			selfWorld = world;
		}

		public void StageTransition(int targetStageId)
		{
			var query = new Query<PlayerCameraComponent, ZoneComponent>(selfWorld);
			query.Fetch();
			query.ForEach((ref PlayerCameraComponent c1, ref ZoneComponent zoneComponent) =>
			{
				zoneComponent.StageId = targetStageId;
			});
		}
	}
}
