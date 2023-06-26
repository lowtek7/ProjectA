using Service.Cursor;
using UnityEngine;
using BlitzEcs;
using Game.Ecs.Component;


namespace UnityService.Input
{
	[UnityService(typeof(ICursorInputService))]
	public class UnityCursorInputService : MonoBehaviour, ICursorInputService
	{
		private Query<CursorComponent> CursorQuery;
		
		[SerializeField] private bool isActiveCursor;
		
		public void Init(BlitzEcs.World world)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			isActiveCursor = false;
			
			CursorQuery = new Query<CursorComponent>(world);
		}
		
		//마우스 커서가 화면에 보이게 할지 여부
		public void ToggleActiveCursor()
		{
			isActiveCursor = !(isActiveCursor);
			
			Cursor.visible = isActiveCursor;
			
			if (isActiveCursor == false)
			{
				Cursor.lockState = CursorLockMode.Locked;
				SetCursorState(false);
				return;
			}
			
			SetCursorState(true);
		}

		//마우스 커서가 화면 밖을 나가게 할지 여부
		public void ToggleLockToScreenCursor()
		{
			Cursor.lockState = isActiveCursor ? CursorLockMode.None : CursorLockMode.Confined;
		}

		private void SetCursorState(bool isActive)
		{
			foreach (var entity in CursorQuery)
			{
				ref var cursorComponent = ref entity.Get<CursorComponent>();

				cursorComponent.IsShowCursor = isActive;
			}
		}
	}
}
