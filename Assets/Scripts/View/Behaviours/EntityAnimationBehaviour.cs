using System;
using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Game.Unit;
using UnityEngine;
using UnityEngine.Serialization;

namespace View.Behaviours
{
	public class EntityAnimationBehaviour : MonoBehaviour, IUnitBehaviour
	{
		private Entity _selfEntity;

		/// <summary>
		/// 캐릭터 애니메이터를 인스펙터에서 찾아야함.
		/// </summary>
		[SerializeField]
		private Animator animator;

		/// <summary>
		/// 시작시 재생할 기본 애니메이션 이름.
		/// </summary>
		[SerializeField]
		private string defaultAnimName;

		public void Connect(Entity entity)
		{
			_selfEntity = entity;

			if (animator is null)
			{
				// 애니메이터가 없으므로 강제로 찾아서 넣어준다.
				animator = gameObject.GetComponentInChildren<Animator>();

				Debug.LogError("Error. Animator is null.");
			}

			animator.Play(defaultAnimName);
		}

		public void Disconnect()
		{
			_selfEntity = new Entity();
		}

		public void Update()
		{
			var deltaTime = Time.deltaTime;

			if (_selfEntity.IsAlive)
			{
				if (_selfEntity.Has<TransformComponent>() && _selfEntity.Has<MovementComponent>())
				{
					var transformComponent = _selfEntity.Get<TransformComponent>();
					var movementComponent = _selfEntity.Get<MovementComponent>();
				}
			}
		}
	}
}
