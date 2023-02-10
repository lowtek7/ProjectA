using System;

namespace Core.Utility
{
	[Flags]
	public enum Direction
	{
		None = 0,
		Up = 1 << 0,
		Right = 1 << 1,
		Down = 1 << 2,
		Left = 1 << 3,
		All = Up | Right | Down | Left,
	}
}
