using System.Collections.Generic;
using BlitzEcs;

namespace Service.Stage
{
	public interface IStagePartitionService : IGameService
	{
		void Fetch();

		void GetBoundsOverlappedEntities(Entity entity, ref List<Entity> overlappedEntities);
	}
}
