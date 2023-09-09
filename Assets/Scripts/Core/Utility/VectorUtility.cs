using UnityEngine;

namespace Core.Utility
{
	public static class VectorUtility
	{
		public static Vector3 GetX0Z(this Vector3 value, float y = 0)
		{
			return new Vector3(value.x, y, value.z);
		}

		public static bool IsAlmostZero(this Vector3 value)
		{
			return value.x.IsAlmostZero() && value.y.IsAlmostZero() && value.z.IsAlmostZero();
		}

		public static bool IsAlmostCloseTo(this Vector3 value, Vector3 target)
		{
			return value.x.IsAlmostCloseTo(target.x) && value.y.IsAlmostCloseTo(target.y) && value.z.IsAlmostCloseTo(target.z);
		}
	}
}
