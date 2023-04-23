using System;
using BlitzEcs;
using Game.Ecs.Component;
using Service;
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

		public void StageTransition(Guid targetStageGuid)
		{
			var query = new Query<PlayerCameraComponent, StageSpecComponent>(selfWorld);

			query.ForEach((ref PlayerCameraComponent c1, ref StageSpecComponent stageSpecComponent) =>
			{
				stageSpecComponent.StageGuid = targetStageGuid;
			});
		}
	}
}
