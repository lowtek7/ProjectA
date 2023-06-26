using Service.Cursor;
using UnityEngine;
using Service;


namespace UnityService.Input
{
	[UnityService(typeof(ICursorInputService))]
	public class UnityCursorInputService : MonoBehaviour, ICursorInputService
	{

		[SerializeField] private bool isActiveCursor;
		
		//마우스 커서가 화면에 보이게 할지 여부
		public void ToggleActiveCursor()
		{
			isActiveCursor = !(isActiveCursor);
			
			Cursor.visible = isActiveCursor;
			
			if (isActiveCursor == false)
			{
				Cursor.lockState = CursorLockMode.Locked;
				return;
			}
			
			
		}

		//마우스 커서가 화면 밖을 나가게 할지 여부
		public void ToggleLockToScreenCursor()
		{
			Cursor.lockState = isActiveCursor ? CursorLockMode.None : CursorLockMode.Confined;
		}

		public void Init(BlitzEcs.World world)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			isActiveCursor = false;
		}
	}
}
