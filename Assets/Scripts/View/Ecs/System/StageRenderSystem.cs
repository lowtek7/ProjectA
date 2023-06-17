using System;
using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Game.World;
using Service;
using Service.Stage;
using View.Ecs.Component;

namespace View.Ecs.System
{
	public class StageRenderSystem : ISystem
	{
		private Query<PlayerCameraComponent, StageSpecComponent> playerQuery;

		private Query<StageRenderComponent> renderQuery;

		private Query<UnitComponent, StageSpecComponent> unitQuery;

		public void Init(World world)
		{
			// 쿼리 조건에 ZoneComponent를 넣는것도 생각 해볼것
			playerQuery = new Query<PlayerCameraComponent, StageSpecComponent>(world);
			renderQuery = new Query<StageRenderComponent>(world);
			unitQuery = new Query<UnitComponent, StageSpecComponent>(world);
			var rendererEntity = world.Spawn();
			rendererEntity.Add(new StageRenderComponent { StageGuid = Guid.Empty });
		}

		public void Update(float deltaTime)
		{
			var currentStageGuid = Constants.UnknownStageGuid;

			foreach (var entity in playerQuery)
			{
				var stageSpecComponent = entity.Get<StageSpecComponent>();

				currentStageGuid = stageSpecComponent.StageGuid;
			}

			if (ServiceManager.TryGetService<IStageRenderService>(out var stageRenderService))
			{
				foreach (var entity in renderQuery)
				{
					ref var stageRenderComponent = ref entity.Get<StageRenderComponent>();

					// 현재 스테이지 Id와 스테이지 렌더 Component의 스테이지 Id가 다르므로 씬이동이 발생해야한다!
					if (stageRenderComponent.StageGuid != currentStageGuid)
					{
						// 스테이지 렌더 컴포넌트의 스테이지 Id를 변경
						stageRenderComponent.StageGuid = currentStageGuid;
						stageRenderService.StageTransition(currentStageGuid);
					}
					else if (!stageRenderService.IsLoading)
					{
						// 씬이동이 발생하지 않으면 모든 엔티티의 ZoneComponent를 조회하여 그려주거나 지워주면 된다.
						foreach (var unitEntity in unitQuery)
						{
							ref var stageSpecComponent = ref unitEntity.Get<StageSpecComponent>();

							// 해당 엔티티가 현재 스테이지와 일치하는지에 대한 검사
							if (stageSpecComponent.StageGuid == currentStageGuid)
							{
								// 만약 일치 한다면 render service에서 현재 그려주고 있는지에 대한 검사를 해야한다.
								if (!stageRenderService.Contains(unitEntity.Id))
								{
									// 그려주고 있지 않다면 그려주게 만든다.
									stageRenderService.StageEnterEvent(unitEntity);
								}
							}
							else
							{
								// 스테이지와 일치하지 않는데 만약 그리고 있다면 스테이지에서 제외 시킨다.
								if (stageRenderService.Contains(unitEntity.Id))
								{
									stageRenderService.StageExitEvent(unitEntity);
								}
							}
						}
					}
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
