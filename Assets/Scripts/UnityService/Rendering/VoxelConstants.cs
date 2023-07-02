using UnityEngine;

namespace UnityService.Rendering
{
	public static class VoxelConstants
	{
		public static readonly Vector3[] VoxelVerts =
		{
			new(0, 0, 0),
			new(1, 0, 0),
			new(1, 1, 0),
			new(0, 1, 0),
			new(0, 0, 1),
			new(1, 0, 1),
			new(1, 1, 1),
			new(0, 1, 1)
		};

		/// <summary>
		/// 순서대로 -Z, +X, +Z, -X, -Y, +Y
		/// </summary>
		public static readonly int[,] VoxelTris =
		{
			{ 0, 3, 1, 2 },
			{ 1, 2, 5, 6 },
			{ 5, 6, 4, 7 },
			{ 4, 7, 0, 3 },
			{ 0, 1, 4, 5 },
			{ 3, 7, 2, 6 }
		};

		public static readonly Vector2[] VoxelUvs =
		{
			new(0, 0),
			new(0, 1),
			new(1, 0),
			new(1, 1),
		};

		public static readonly Vector3Int[] NearVoxels =
		{
			Vector3Int.back,
			Vector3Int.right,
			Vector3Int.forward,
			Vector3Int.left,
			Vector3Int.down,
			Vector3Int.up,
		};

		public static readonly int BlockSideCount = VoxelTris.GetLength(0);
		public static readonly int VertexInSideCount = VoxelTris.GetLength(1);

		public static readonly int ChunkAxisExponent = 4;

		public static readonly int ChunkAxisCount = 1 << ChunkAxisExponent;
	}
}
