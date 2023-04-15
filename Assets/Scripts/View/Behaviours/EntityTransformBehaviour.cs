using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Game.Unit;
using UnityEngine;

namespace View.Behaviours
{
	/// <summary>
	/// 해당 비헤이비어를 부착시키면 매 업데이트마다 관측중인 엔티티의 Transform을 유니티 Transform에 반영합니다.
	/// </summary>
	public class EntityTransformBehaviour : CustomBehaviour, IUnitBehaviour, IUpdate
	{
		[SerializeField]
		private Transform spriteTransform;

		private Entity _selfEntity;

		public void Connect(Entity entity)
		{
			_selfEntity = entity;
		}

		public void Disconnect()
		{
			_selfEntity = new Entity();
		}

		public void UpdateProcess(float deltaTime)
		{
			if (_selfEntity.IsAlive)
			{
				if (_selfEntity.Has<TransformComponent>())
				{
					var transformComponent = _selfEntity.Get<TransformComponent>();
					var direction2d = transformComponent.Direction2D;
					var prevScale = spriteTransform.localScale;
					var scaleX = Mathf.Abs(prevScale.x);

					transform.position = transformComponent.Position;

					if ((direction2d & Direction2D.Left) != 0)
					{
						spriteTransform.localScale = new Vector3(scaleX * -1, prevScale.y, prevScale.z);
					}
					else if ((direction2d & Direction2D.Right) != 0)
					{
						spriteTransform.localScale = new Vector3(scaleX, prevScale.y, prevScale.z);
					}
				}
			}
		}
	}
}
