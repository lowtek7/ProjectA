using UnityEngine;

namespace UnityService.Rendering
{
	public static class VoxelUtility
	{
		public static int GetCoordAxis(float value)
		{
			return Mathf.FloorToInt(value) >> VoxelConstants.ChunkAxisExponent;
		}

		/// <summary>
		/// 청크의 좌표(Transform이 아닌 int형 좌표)를 이용해 ID를 뽑아냄
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static int GetCoordId(int x, int y, int z)
		{
			return ((x + VoxelConstants.ChunkCoordXOffset) << VoxelConstants.ChunkCoordXExponent) |
			       ((y + VoxelConstants.ChunkCoordYOffset) << VoxelConstants.ChunkCoordYExponent) |
			       ((z + VoxelConstants.ChunkCoordZOffset) << VoxelConstants.ChunkCoordZExponent);
		}

		public static int GetCoordX(int coordId)
		{
			return ((coordId & VoxelConstants.ChunkCoordXBitRange) >> VoxelConstants.ChunkCoordXExponent) -
			       VoxelConstants.ChunkCoordXOffset;
		}

		public static int GetCoordY(int coordId)
		{
			return ((coordId & VoxelConstants.ChunkCoordYBitRange) >> VoxelConstants.ChunkCoordYExponent) -
			       VoxelConstants.ChunkCoordYOffset;
		}

		public static int GetCoordZ(int coordId)
		{
			return ((coordId & VoxelConstants.ChunkCoordZBitRange) >> VoxelConstants.ChunkCoordZExponent) -
			       VoxelConstants.ChunkCoordZOffset;
		}

		public static int GetCoordSqrDistance(int aId, int bId)
		{
			var ax = GetCoordX(aId);
			var ay = GetCoordY(aId);
			var az = GetCoordZ(aId);

			var bx = GetCoordX(bId);
			var by = GetCoordY(bId);
			var bz = GetCoordZ(bId);

			var xDiff = ax - bx;
			var yDiff = ay - by;
			var zDiff = az - bz;

			return xDiff * xDiff + yDiff * yDiff + zDiff * zDiff;
		}

		public static int MoveCoord(int coordId, int xDiff, int yDiff, int zDiff)
		{
			var x = GetCoordX(coordId) + xDiff;
			var y = GetCoordY(coordId) + yDiff;
			var z = GetCoordZ(coordId) + zDiff;

			return GetCoordId(x, y, z);
		}
	}
}
