﻿using UnityEngine;

namespace Game.World.Stage
{
	public static class ChunkConstants
	{
		public static readonly int ChunkCoordZExponent = 0;
		private static readonly int ChunkCoordZBitCount = 4;
		public static readonly int ChunkCoordZOffset = 1 << (ChunkCoordZBitCount - 1);
		public static readonly int ChunkCoordZBitRange = 0xF;

		public static readonly int ChunkCoordYExponent = ChunkCoordZExponent + ChunkCoordZBitCount;
		private static readonly int ChunkCoordYBitCount = 4;
		public static readonly int ChunkCoordYOffset = 1 << (ChunkCoordYBitCount - 1);
		public static readonly int ChunkCoordYBitRange = 0xF0;

		public static readonly int ChunkCoordXExponent = ChunkCoordYExponent + ChunkCoordYBitCount;
		private static readonly int ChunkCoordXBitCount = 4;
		public static readonly int ChunkCoordXOffset = 1 << (ChunkCoordXBitCount - 1);
		public static readonly int ChunkCoordXBitRange = 0xF00;

		public static readonly int TotalChunkBitCount = ChunkCoordXBitCount + ChunkCoordYBitCount + ChunkCoordZBitCount;
		public static readonly int TotalChunkCount = 1 << TotalChunkBitCount;

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

		public static readonly Vector3Int[] NearVoxels =
		{
			Vector3Int.right,
			Vector3Int.left,
			Vector3Int.up,
			Vector3Int.down,
			Vector3Int.forward,
			Vector3Int.back,
		};

		public static readonly int LocalBlockAxisExponent = 4;
		public static readonly int LocalBlockAxisCount = 1 << LocalBlockAxisExponent;
		public static readonly int MaxLocalBlockCount = LocalBlockAxisCount * LocalBlockAxisCount * LocalBlockAxisCount;

		public static readonly ushort InvalidBlockId = ushort.MaxValue;

		public static readonly int BlockSideCount = 6;

		public static readonly int LocalBlockXBitRange = (LocalBlockAxisCount - 1) << (LocalBlockAxisExponent << 1);
		public static readonly int LocalBlockYBitRange = (LocalBlockAxisCount - 1) << (LocalBlockAxisExponent);
		public static readonly int LocalBlockZBitRange = LocalBlockAxisCount - 1;
	}
}
