using UnityEngine;

namespace Service.Stage
{
	public interface IChunkService : IGameService
	{
		// void Fetch();
		bool IsSolidAtOuter(Vector3Int chunkCoord, Vector3Int localPos);
	}
}
