using Core.Unity;
using Service;
using Service.Stage;

namespace Game.Ecs.System
{
	public class StagePartitionSystem : ISystem
	{
		public void Init(BlitzEcs.World world)
		{
		}

		public void Update(float deltaTime)
		{
			if (ServiceManager.TryGetService<IStagePartitionService>(out var partitionService))
			{
				partitionService.Fetch();
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
