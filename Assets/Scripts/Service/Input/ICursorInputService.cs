namespace Service.Cursor
{
	public interface ICursorInputService : IGameService
	{
		//마우스 커서가 화면에 보이게 할지 여부
		void ToggleActiveCursor();

		//마우스 커서가 화면 밖을 나가게 할지 여부
		void ToggleLockToScreenCursor();
	}
}
