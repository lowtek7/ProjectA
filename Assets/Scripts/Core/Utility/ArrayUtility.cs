namespace Core.Utility
{
	public static class ArrayUtility
	{
		public static T[] CreateArrayFilledWith<T>(int size, T value)
		{
			var array = new T[size];

			for (int i = 0; i < size; i++)
			{
				array[i] = value;
			}

			return array;
		}

	}
}
