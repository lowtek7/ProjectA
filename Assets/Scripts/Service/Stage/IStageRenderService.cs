using System;
using BlitzEcs;

namespace Service.Stage
{
	public interface IStageRenderService : IGameService
	{
		bool IsLoading { get; }

		bool Contains(int entityId);

		void StageTransition(Guid stageGuid);

		void StageEnterEvent(Entity entity);

		void StageExitEvent(Entity entity);
	}
}
