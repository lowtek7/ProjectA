using System;
using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Game.Unit;
using Library.JSAnim2D;
using UnityEngine;

namespace View.Behaviours
{
	public class EntityAnimationBehaviour : CustomBehaviour, IUnitBehaviour, IUpdate
	{
		private Entity _selfEntity;

		private JSAnimator[] _animators = Array.Empty<JSAnimator>();

		private static string[] Directions => new[] { "down", "left", "right", "up" };

		public void Connect(Entity entity)
		{
			_selfEntity = entity;
			_animators = gameObject.GetComponentsInChildren<JSAnimator>();
		}

		public void Disconnect()
		{
			_selfEntity = new Entity();
		}

		public void UpdateProcess(float deltaTime)
		{
			if (_selfEntity.IsAlive)
			{
				if (_selfEntity.Has<TransformComponent>() && _selfEntity.Has<MovementComponent>())
				{
					var transformComponent = _selfEntity.Get<TransformComponent>();
					var movementComponent = _selfEntity.Get<MovementComponent>();

					if (movementComponent.MoveDir != Vector3.zero)
					{
						var moveAnimStr = $"walk_{transformComponent.DirectionName}";

						foreach (var animator in _animators)
						{
							animator.Play(moveAnimStr);
						}
					}
					else
					{
						var idleAnimStr = $"idle_{transformComponent.DirectionName}";

						foreach (var animator in _animators)
						{
							animator.Play(idleAnimStr);
						}
					}
				}
			}

			foreach (var animator in _animators)
			{
				// 나중에 speed도 시간값에 곱하는것을 고려하기
				animator.AnimationUpdate(deltaTime);
			}
		}
	}
}
