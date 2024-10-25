﻿using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Game.Unit;
using Service;
using Service.Audio;
using UnityEngine;

namespace View.Behaviours
{
	/// <summary>
	/// 해당 비헤이비어를 부착시키면 매 업데이트마다 관측중인 엔티티의 Transform을 유니티 Transform에 반영합니다.
	/// </summary>
	public class EntityTransformBehaviour : CustomBehaviour, IUnitBehaviour, IUpdate
	{
		private Entity _selfEntity;

		private Transform _transform;

		public void Connect(Entity entity)
		{
			_selfEntity = entity;
			_transform = transform;
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

					_transform.position = transformComponent.Position;
					_transform.rotation = transformComponent.Rotation;
				}
			}
		}
	}
}
