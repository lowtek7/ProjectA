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
		/// 순서대로 +X, -X, +Y, -Y, +Z, -Z
		/// </summary>
		public static readonly int[,] VoxelTris =
		{
			{ 1, 2, 5, 6 },	// right
			{ 4, 7, 0, 3 },	// left
			{ 3, 7, 2, 6 },	// up
			{ 0, 1, 4, 5 },	// down
			{ 5, 6, 4, 7 },	// front
			{ 0, 3, 1, 2 },	// back
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
			Vector3Int.right,
			Vector3Int.left,
			Vector3Int.up,
			Vector3Int.down,
			Vector3Int.forward,
			Vector3Int.back,
		};

		public static readonly int BlockSideCount = VoxelTris.GetLength(0);
		public static readonly int VertexInSideCount = VoxelTris.GetLength(1);
	}
}
