using BlitzEcs;
using Core.Unity;
using Core.Utility;
using Game.Ecs.Component;
using Service;
using Service.Camera;
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
			if (!ServiceManager.TryGetService<IPlayerCameraService>(out var playerCameraService))
			{
				return;
			}

			if (!ServiceManager.TryGetService<IFieldRenderService>(out var fieldRenderService))
			{
				return;
			}

			_playerCameraQuery.ForEach(((ref PlayerCameraComponent playerCamera, ref TransformComponent cameraTransform) =>
			{
				var screenBounds = new Bounds(cameraTransform.Position, playerCameraService.WorldSize);

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

					// 바운드나 렌더링용 바운드가 존재할 때에만 실행
					if (bounds.HasValue)
					{
						var renderBounds = bounds.Value;
						var isRendering = fieldRenderService.IsRendering(entity);
						var intersects = renderBounds.IntersectXY(screenBounds);

						if (!isRendering && intersects)
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
						else if (isRendering && !intersects)
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

			fieldRenderService.Fetch();
		}
	}
}
