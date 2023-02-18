using UnityEngine;

namespace View.Manager
{
	public interface ICursor
	{
		Vector2 ScreenPos { get; }
		Vector3 WorldPos { get; }
	}

	/// <summary>
	/// 커서에 대한 정보를 관리하고 있는 매니저
	/// 실제 커서 객체가 셋팅되기 전에는 더미 커서가 포지션에 대한 정보를 반환 하고있다.
	/// </summary>
	public static class CursorManager
	{
		private static ICursor cursorInstance = new DummyCursor();

		public static Vector2 ScreenPosition => cursorInstance.ScreenPos;

		public static Vector3 WorldPosition => cursorInstance.WorldPos;

		public static void SetCursor(ICursor cursor)
		{
			cursorInstance = cursor;
		}

		public static void ClearCursor()
		{
			cursorInstance = new DummyCursor();
		}

		class DummyCursor : ICursor
		{
			public Vector2 ScreenPos => Vector2.zero;
			public Vector3 WorldPos => Vector3.zero;
		}
	}
}
