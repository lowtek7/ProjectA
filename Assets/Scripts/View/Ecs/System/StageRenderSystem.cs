using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Game.World;
using View.Ecs.Component;
using View.Service;

namespace View.Ecs.System
{
	public class StageRenderSystem : ISystem
	{
		public int Order => 100;

		private Query<PlayerCameraComponent> playerQuery;

		private Query<StageRenderComponent> renderQuery;

		public void Init(World world)
		{
			// 쿼리 조건에 ZoneComponent를 넣는것도 생각 해볼것
			playerQuery = new Query<PlayerCameraComponent>(world);
			renderQuery = new Query<StageRenderComponent>(world);
		}

		public void Update(float deltaTime)
		{
			// 쿼리 사용전에는 무조건 Fetch 할 것.
			playerQuery.Fetch();
			renderQuery.Fetch();

			int currentStageId = Constants.UnknownStageId;

			foreach (var entity in playerQuery)
			{
				if (entity.Has<ZoneComponent>())
				{
					// 읽기전용으로 가져올 생각이기 때문에 ref를 사용하지 않는다.
					var zoneComponent = entity.Get<ZoneComponent>();

					currentStageId = zoneComponent.StageId;
				}
			}

			foreach (var entity in renderQuery)
			{
				ref var stageRenderComponent = ref entity.Get<StageRenderComponent>();

				// 현재 스테이지 Id와 스테이지 렌더 Component의 스테이지 Id가 다르므로 씬이동이 발생해야한다!
				if (stageRenderComponent.StageId != currentStageId)
				{
					// 스테이지 렌더 컴포넌트의 스테이지 Id를 변경
					stageRenderComponent.StageId = currentStageId;
					if (StageRenderService.TryGetInstance(out var stageRenderService))
					{
						stageRenderService.StageTransition(currentStageId);
					}
				}
				break;
			}
		}
	}
}
