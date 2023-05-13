using UnityEngine;

namespace Core.Utility
{
	public static class BoundsUtility
	{
		public static Vector3[] GetPoints(this Bounds bounds)
		{
			var min = bounds.min;
			var max = bounds.max;

			return new[]
			{
				min,
				max,
				new Vector3(min.x, min.y, max.z),
				new Vector3(min.x, max.y, min.z),
				new Vector3(max.x, min.y, min.z),
				new Vector3(min.x, max.y, max.z),
				new Vector3(max.x, min.y, max.z),
				new Vector3(max.x, max.y, min.z)
			};
		}
	}
}
