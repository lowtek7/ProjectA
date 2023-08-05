using BlitzEcs;
using Core.Unity;
using Core.Utility;
using Game.Ecs.Component;
using Game.Extensions;
using Service;
using Service.Command;
using UnityEngine;

namespace Game.Ecs.System
{
	public class GravitySystem : ISystem
	{
		private Query<TransformComponent, GravityComponent> _gravityQuery;

		public void Init(BlitzEcs.World world)
		{
			_gravityQuery = new Query<TransformComponent, GravityComponent>(world);
		}

		public void Update(float deltaTime)
		{
			foreach (var entity in _gravityQuery)
			{
				if (entity.IsRemoteEntity())
				{
					continue;
				}

				ref var transformComponent = ref entity.Get<TransformComponent>();

				var dist = (-1.0f * deltaTime);

				// 임시로 0까지만 중력 받도록 함
				if (transformComponent.Position.y + dist > 0.0f)
				{
					transformComponent.Position += Vector3.up * dist;
				}
				else
				{
					transformComponent.Position = transformComponent.Position.GetX0Z();
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
