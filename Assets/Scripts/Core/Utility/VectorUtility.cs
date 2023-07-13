using UnityEngine;

namespace Core.Utility
{
	public static class VectorUtility
	{
		public static Vector3 GetX0Z(this Vector3 value, float y = 0)
		{
			return new Vector3(value.x, y, value.z);
		}
	}
}
