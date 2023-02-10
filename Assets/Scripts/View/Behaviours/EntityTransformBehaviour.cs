using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Game.Unit;

namespace View.Behaviours
{
	/// <summary>
	/// 해당 비헤이비어를 부착시키면 매 업데이트마다 관측중인 엔티티의 Transform을 유니티 Transform에 반영합니다.
	/// </summary>
	public class EntityTransformBehaviour : CustomBehaviour, IUnitBehaviour, IUpdate
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

		public void UpdateProcess(float deltaTime)
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
