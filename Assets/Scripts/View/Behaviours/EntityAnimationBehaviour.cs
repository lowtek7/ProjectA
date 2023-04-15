using System;
using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Game.Unit;
using Library.JSAnim2D;
using UnityEngine;

namespace View.Behaviours
{
	public enum Direction
	{
		Down = 0,
		Left = 1,
		Right = 2,
		Up = 3
	}
	
	public class EntityAnimationBehaviour : CustomBehaviour, IUnitBehaviour, IUpdate
	{
		private Entity _selfEntity;

		private JSAnimator[] _animators = Array.Empty<JSAnimator>();

		private Direction _currentDir = DefaultDirection;

		private static Direction DefaultDirection => Direction.Down;

		private static string[] Directions => new[] { "down", "left", "right", "up" };

		public void Connect(Entity entity)
		{
			_selfEntity = entity;
			_animators = gameObject.GetComponentsInChildren<JSAnimator>();
			_currentDir = DefaultDirection;
		}

		public void Disconnect()
		{
			_selfEntity = new Entity();
			_currentDir = DefaultDirection;
		}

		public void UpdateProcess(float deltaTime)
		{
			if (_selfEntity.IsAlive)
			{
				if (_selfEntity.Has<MovementComponent>())
				{
					var movementComponent = _selfEntity.Get<MovementComponent>();

					if (movementComponent.MoveDir != Vector3.zero)
					{
						var moveDir = movementComponent.MoveDir;

						if (moveDir.x > 0)
						{
							_currentDir = Direction.Right;
						}
						else if (moveDir.x < 0)
						{
							_currentDir = Direction.Left;
						}
						else if (moveDir.y > 0)
						{
							_currentDir = Direction.Up;
						}
						else if (moveDir.y < 0)
						{
							_currentDir = Direction.Down;
						}
						else
						{
							_currentDir = DefaultDirection;
						}
						
						var moveAnimStr = $"walk_{Directions[(int)_currentDir]}";

						foreach (var animator in _animators)
						{
							animator.Play(moveAnimStr);
						}
					}
					else
					{
						var idleAnimStr = $"idle_{Directions[(int)_currentDir]}";

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
