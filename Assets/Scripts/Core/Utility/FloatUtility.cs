using UnityEngine;

namespace Core.Utility
{
	public static class FloatUtility
	{
		public static float Epsilon => 0.001f;

		public static bool IsAlmostZero(this float value)
		{
			return Mathf.Abs(value) <= Epsilon;
		}

		public static bool IsAlmostCloseTo(this float value, float target)
		{
			return Mathf.Abs(value - target) <= Epsilon;
		}
	}
}
