using Service.Texture;
using UnityEngine;

namespace Service.Rendering
{
	public interface IChunkService : IGameService
	{
		// void Fetch();
		bool IsSolidAtOuter(Vector3Int chunkCoord, Vector3Int localPos);

		bool TryGetUvInfo(string blockName, int sideIndex, out PackedTextureUvInfo info);
	}
}
