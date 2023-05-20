using BlitzEcs;
using Core.Unity;
using Core.Utility;
using Game.Ecs.Component;
using Service;
using Service.Field;
using UnityEngine;
using View.Ecs.Component;

namespace View.Ecs.System
{
	public class FieldRenderSystem : ISystem
	{
		private Query<FieldCellComponent, TransformComponent> _fieldCellQuery;

		private Query<PlayerCameraComponent, TransformComponent> _playerCameraQuery;

		public void Init(World world)
		{
			_fieldCellQuery = new Query<FieldCellComponent, TransformComponent>(world);
			_playerCameraQuery = new Query<PlayerCameraComponent, TransformComponent>(world);
		}

		public void Update(float deltaTime)
		{
		}

		public void LateUpdate(float deltaTime)
		{
			_playerCameraQuery.ForEach(((ref PlayerCameraComponent playerCamera, ref TransformComponent cameraTransform) =>
			{
				// TODO : CameraService를 만들어서 PlayerCameraComponent의 데이터 갱신에 따라 카메라 유니티 컴포넌트의 수치를 조정해주어야 함
				// 일단 사이즈가 4로 고정되어있다고 생각하고 작업하자.
				float cameraSize = 4f;
				float aspect = 16f / 9f;
				float screenHeight = cameraSize * 2f;
				float screenWidth = screenHeight * aspect;

				var screenSize = new Vector3(screenWidth, screenHeight, 0);
				var screenBounds = new Bounds(cameraTransform.Position, screenSize);

				_fieldCellQuery.ForEach(((Entity entity, ref FieldCellComponent fieldCell, ref TransformComponent cellTransform) =>
				{
					Bounds? bounds = null;

					if (entity.Has<BoundsComponent>())
					{
						bounds = entity.Get<BoundsComponent>().GetBounds(cellTransform.Position);
					}
					else if (entity.Has<FieldRenderBoundsComponent>())
					{
						bounds = entity.Get<FieldRenderBoundsComponent>().GetBounds(cellTransform.Position);
					}

					if (bounds.HasValue)
					{
						var renderBounds = bounds.Value;

						if (renderBounds.IntersectXY(screenBounds))
						{
							// 화면과 겹치는 경우, 활성화 요청을 붙여준다.
							if (!entity.Has<FieldCellRequestRealizeComponent>())
							{
								entity.Add(new FieldCellRequestRealizeComponent());
							}

							if (entity.Has<FieldCellRequestVirtualizeComponent>())
							{
								entity.Remove<FieldCellRequestVirtualizeComponent>();
							}
						}
						else
						{
							// 화면과 겹치지 않으면, 비활성화 요청을 붙여준다.
							if (!entity.Has<FieldCellRequestVirtualizeComponent>())
							{
								entity.Add(new FieldCellRequestVirtualizeComponent());
							}

							if (entity.Has<FieldCellRequestRealizeComponent>())
							{
								entity.Remove<FieldCellRequestRealizeComponent>();
							}
						}
					}
				}));
			}));

			if (ServiceManager.TryGetService<IFieldRenderService>(out var fieldRenderService))
			{
				fieldRenderService.Fetch();
			}
		}
	}
}
