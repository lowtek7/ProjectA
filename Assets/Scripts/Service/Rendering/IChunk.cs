using UnityEngine;

namespace Service.Rendering
{
	public enum ChunkState
	{
		None,
		WaitBuild,
		Building,
		Done
	}

	public interface IChunk
	{
		void Initialize(int coordId);

		void RebuildMesh(bool[] isSolidLeft, bool[] isSolidRight, bool[] isSolidUp,
			bool[] isSolidDown, bool[] isSolidForward, bool[] isSolidBack);

		bool IsSolidAt(int x, int y, int z);

		void UpdateBuildMesh();

		int CoordId { get; }

		GameObject GameObject { get; }

		bool[] IsSolidMap { get; }

		ChunkState State { get; }
	}
}
