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

		public static readonly int ChunkAxisExponent = 4;
		public static readonly int ChunkAxisCount = 1 << ChunkAxisExponent;

		public static readonly int ChunkCoordZExponent = 0;
		private static readonly int ChunkCoordZBitCount = 8;
		public static readonly int ChunkCoordZOffset = 1 << (ChunkCoordZBitCount - 1);
		public static readonly int ChunkCoordZBitRange = 0xFF;

		public static readonly int ChunkCoordYExponent = ChunkCoordZExponent + ChunkCoordZBitCount;
		private static readonly int ChunkCoordYBitCount = 4;
		public static readonly int ChunkCoordYOffset = 1 << (ChunkCoordYBitCount - 1);
		public static readonly int ChunkCoordYBitRange = 0xF00;

		public static readonly int ChunkCoordXExponent = ChunkCoordYExponent + ChunkCoordYBitCount;
		private static readonly int ChunkCoordXBitCount = 8;
		public static readonly int ChunkCoordXOffset = 1 << (ChunkCoordXBitCount - 1);
		public static readonly int ChunkCoordXBitRange = 0xFF000;

		public static readonly int TotalChunkBitCount = ChunkCoordXBitCount + ChunkCoordYBitCount + ChunkCoordZBitCount;

		public static readonly int InvalidCoordId = ChunkCoordXBitRange + ChunkCoordYBitRange + ChunkCoordZBitRange + 1;

		public static readonly int[] NearCoordAdders =
		{
			1 << ChunkCoordXExponent,		// Right
			-(1 << ChunkCoordXExponent),	// Left
			1 << ChunkCoordYExponent,		// Up
			-(1 << ChunkCoordYExponent),	// Down
			1 << ChunkCoordZExponent,		// Forward
			-(1 << ChunkCoordZExponent),	// Back
		};
	}
}
