using BlitzEcs;
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
		[SerializeField]
		private Transform spriteTransform;

		private Entity _selfEntity;

		/// <summary>
		/// sfx 테스트용 쿨다운 타임
		/// </summary>
		private readonly float _sfxCooldownTime = 5;

		/// <summary>
		/// sfx 테스트용 현재 쿨다운 시간
		/// </summary>
		private float _currentCooldownTime;

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
			if (_currentCooldownTime > 0)
			{
				_currentCooldownTime -= deltaTime;
			}

			if (_selfEntity.IsAlive)
			{
				if (_selfEntity.Has<TransformComponent>())
				{
					var transformComponent = _selfEntity.Get<TransformComponent>();
					var direction2d = transformComponent.Direction2D;
					var prevScale = spriteTransform.localScale;
					var scaleX = Mathf.Abs(prevScale.x);

					if (_currentCooldownTime <= 0 && transform.position != transformComponent.Position)
					{
						_currentCooldownTime = _sfxCooldownTime;

						if (ServiceManager.TryGetService(out IAudioService audioService))
						{
							audioService.TestPlayOneShotSFX("get_item", transformComponent.Position);
						}
					}

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
