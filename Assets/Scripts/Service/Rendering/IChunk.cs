using UnityEngine;

namespace Service.Rendering
{
	public interface IChunk
	{
		void Initialize(Vector3Int coord);

		void RebuildMesh(bool[] isSolidLeft, bool[] isSolidRight, bool[] isSolidUp,
			bool[] isSolidDown, bool[] isSolidForward, bool[] isSolidBack);

		bool IsSolidAt(int x, int y, int z);

		bool UpdateBuildMesh();

		Vector3Int Coord { get; }

		GameObject GameObject { get; }

		bool[] IsSolidMap { get; }

		bool NeedWaitBuildMesh { get; }
	}
}
