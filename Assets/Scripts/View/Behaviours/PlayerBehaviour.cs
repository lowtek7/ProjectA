using BlitzEcs;
using Game.Ecs.Component;

namespace View.Behaviours
{
	public class PlayerBehaviour : EcsSuperBehaviour, IComponentBinder<PlayerComponent>
	{
		public override void SetupEntity(Entity entity)
		{
			base.SetupEntity(entity);
			
			// 하고싶은 작업 여기서 하면 된다.
			ref var playerComponent = ref entity.Get<PlayerComponent>();
		}
	}
}
