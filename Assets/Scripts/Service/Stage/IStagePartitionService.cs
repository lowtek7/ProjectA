using System.Collections.Generic;
using BlitzEcs;

namespace Service.Stage
{
	public struct PartitionBoundsInfo
	{
		/// <summary>
		/// 사용 편의성을 위해 Id가 아닌 Entity로 뱉도록했는데, 문제가 된다면 Id로 교체 필요
		/// </summary>
		public Entity entity;

		/// <summary>
		/// 바운드의 인덱스
		/// </summary>
		public int boundsIndex;

		public override int GetHashCode()
		{
			return entity.Id.GetHashCode() * 9719 + boundsIndex.GetHashCode();
		}
	}

	public interface IStagePartitionService : IGameService
	{
		void Fetch();

		void GetBoundsOverlappedEntities(Entity entity, ref List<PartitionBoundsInfo> overlappedBoundsInfos);
	}
}
