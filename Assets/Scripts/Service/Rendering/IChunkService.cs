using Service.Texture;
using UnityEngine;

namespace Service.Rendering
{
	public interface IChunkService : IGameService
	{
		bool IsSolidAt(IChunk chunk, int x, int y, int z);

		bool TryGetUvInfo(string blockName, int sideIndex, out PackedTextureUvInfo info);
	}
}
