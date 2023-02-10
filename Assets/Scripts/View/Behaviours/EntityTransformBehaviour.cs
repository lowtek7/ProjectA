using BlitzEcs;
using Game.Ecs.Component;
using Game.Unit;
using UnityEngine;

namespace View.Behaviours
{
	public class EntityTransformBehaviour : MonoBehaviour, IUnitBehaviour
	{
		private Entity selfEntity;

		public void Connect(Entity entity)
		{
			selfEntity = entity;
		}

		public void Disconnect()
		{
			selfEntity = new Entity();
		}

		public void Update()
		{
			if (selfEntity.IsAlive)
			{
				if (selfEntity.Has<TransformComponent>())
				{
					var transformComponent = selfEntity.Get<TransformComponent>();
					transform.position = transformComponent.Position;
				}
			}
		}
	}
}
