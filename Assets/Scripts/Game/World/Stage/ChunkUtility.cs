using UnityEngine;

namespace Game.World.Stage
{
	public static class ChunkUtility
	{
		public static int GetCoordAxis(float worldValue)
		{
			return Mathf.RoundToInt(worldValue) >> ChunkConstants.LocalBlockAxisExponent;
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
			return ((x + ChunkConstants.ChunkCoordXOffset) << ChunkConstants.ChunkCoordXExponent) |
			       ((y + ChunkConstants.ChunkCoordYOffset) << ChunkConstants.ChunkCoordYExponent) |
			       ((z + ChunkConstants.ChunkCoordZOffset) << ChunkConstants.ChunkCoordZExponent);
		}

		public static int GetCoordX(int coordId)
		{
			return ((coordId & ChunkConstants.ChunkCoordXBitRange) >> ChunkConstants.ChunkCoordXExponent) -
			       ChunkConstants.ChunkCoordXOffset;
		}

		public static int GetCoordY(int coordId)
		{
			return ((coordId & ChunkConstants.ChunkCoordYBitRange) >> ChunkConstants.ChunkCoordYExponent) -
			       ChunkConstants.ChunkCoordYOffset;
		}

		public static int GetCoordZ(int coordId)
		{
			return ((coordId & ChunkConstants.ChunkCoordZBitRange) >> ChunkConstants.ChunkCoordZExponent) -
			       ChunkConstants.ChunkCoordZOffset;
		}

		public static Vector3Int ConvertIdToPos(int coordId)
		{
			var x = GetCoordX(coordId);
			var y = GetCoordY(coordId);
			var z = GetCoordZ(coordId);

			return new Vector3Int(x, y, z);
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

		public static bool TryMoveCoord(int coordId, int xDiff, int yDiff, int zDiff, out int movedCoordId)
		{
			var x = GetCoordX(coordId) + xDiff;
			var y = GetCoordY(coordId) + yDiff;
			var z = GetCoordZ(coordId) + zDiff;

			if (x >= -ChunkConstants.ChunkCoordXOffset && x < ChunkConstants.ChunkCoordXOffset &&
			    y >= -ChunkConstants.ChunkCoordYOffset && y < ChunkConstants.ChunkCoordYOffset &&
			    z >= -ChunkConstants.ChunkCoordZOffset && z < ChunkConstants.ChunkCoordZOffset)
			{
				movedCoordId = GetCoordId(x, y, z);

				return true;
			}

			movedCoordId = ChunkConstants.InvalidCoordId;

			return false;
		}

		public static Vector3Int GetLocalBlockOffset(int localId)
		{
			return new Vector3Int(
				localId & ChunkConstants.LocalBlockXBitRange,
				localId & ChunkConstants.LocalBlockYBitRange,
				localId & ChunkConstants.LocalBlockZBitRange
				);
		}

		public static int GetLocalBlockId(int localX, int localY, int localZ)
		{
			var xWeight = localX << (ChunkConstants.LocalBlockAxisExponent << 1);
			var yWeight = localY << ChunkConstants.LocalBlockAxisExponent;

			return xWeight | yWeight | localZ;
		}
	}
}
