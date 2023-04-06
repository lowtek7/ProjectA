using System;
using BlitzEcs;
using Game.Service;

namespace View.Service
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
