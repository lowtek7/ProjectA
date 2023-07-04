using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace RAMGUnsafe
{
	public static class UnsafeUtility
	{
		public static unsafe void CopyToFast<T>(
			this NativeSlice<T> nativeSlice,
			T[] array)
			where T : struct
		{
			if (array == null)
			{
				throw new NullReferenceException(nameof(array) + " is null");
			}
			int nativeArrayLength = nativeSlice.Length;
			if (array.Length < nativeArrayLength)
			{
				throw new IndexOutOfRangeException(
					nameof(array) + " is shorter than " + nameof(nativeSlice));
			}
			int byteLength = nativeSlice.Length * Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<T>();
			void* managedBuffer = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref array[0]);
			void* nativeBuffer = nativeSlice.GetUnsafePtr();
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy(managedBuffer, nativeBuffer, byteLength);
		}
	}
}
