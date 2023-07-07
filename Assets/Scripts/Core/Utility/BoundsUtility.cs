using UnityEngine;

namespace Core.Utility
{
	public static class BoundsUtility
	{
		public static void GetPoints(this Bounds bounds, ref Vector3[] outPoints)
		{
			var min = bounds.min;
			var max = bounds.max;

			outPoints[0] = min;
			outPoints[1] = max;
			outPoints[2] = new Vector3(min.x, min.y, max.z);
			outPoints[3] = new Vector3(min.x, max.y, min.z);
			outPoints[4] = new Vector3(max.x, min.y, min.z);
			outPoints[5] = new Vector3(min.x, max.y, max.z);
			outPoints[6] = new Vector3(max.x, min.y, max.z);
			outPoints[7] = new Vector3(max.x, max.y, min.z);
		}

		public static bool IntersectXY(this Bounds bounds, Bounds other)
		{
			return bounds.max.x > other.min.x && bounds.min.x < other.max.x &&
			       bounds.max.y > other.min.y && bounds.min.y < other.max.y;
		}
	}
}
