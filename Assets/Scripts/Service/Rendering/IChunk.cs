using UnityEngine;

namespace Service.Rendering
{
	public interface IChunk
	{
		void Initialize(Vector3Int coord);

		void RebuildMesh();

		bool IsSolidAt(int x, int y, int z);

		Vector3Int Coord { get; }

		GameObject GameObject { get; }
	}
}
