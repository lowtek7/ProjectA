using BlitzEcs;
using Game.Ecs.Component;
using Service;
using Service.Camera;
using Service.Cursor;
using Service.Input;
using UnityEngine;

namespace UnityService.Input
{
	[UnityService(typeof(IGameInputService))]
	public class KeyboardAndMouseGameInputService : MonoBehaviour, IGameInputService
	{
		private World selfWorld;

		private Query<InputComponent> inputQuery;

		private Query<CursorComponent> CursorQuery;

		private Query<PlayerCameraComponent> cameraQuery;

		// 사용하지 않고 있으며 경고가 나와서 주석처리
		//private bool activated = false;

		private Entity inputEntity;

		public void Init(World world)
		{
			selfWorld = world;
			inputQuery = new Query<InputComponent>(world);

			CursorQuery = new Query<CursorComponent>(world);

			cameraQuery = new Query<PlayerCameraComponent>(world);

			// 인풋 관리는 한 곳에서만 진행할 것이므로,
			inputEntity = world.Spawn();
			inputEntity.Add(new InputComponent{ MoveDirection = Vector3.zero });
			inputEntity.Add(new CursorComponent{ IsShowCursor = false });
		}

		void IGameInputService.Fetch()
		{
			int xMove = 0;
			int yMove = 0;
			int zMove = 0;

			float mouseXDegree = 0.0f;
			float mouseYDegree = 0.0f;

			bool isMouseClick = false;

			bool isRun = false;

			if (UnityEngine.Input.GetKey(KeyCode.W))
			{
				zMove += 1;
			}

			if (UnityEngine.Input.GetKey(KeyCode.S))
			{
				zMove -= 1;
			}

			if (UnityEngine.Input.GetKey(KeyCode.D))
			{
				xMove += 1;
			}

			if (UnityEngine.Input.GetKey(KeyCode.A))
			{
				xMove -= 1;
			}

			if (UnityEngine.Input.GetKey(KeyCode.Space))
			{
				yMove += 1;
			}

			if (UnityEngine.Input.GetKey(KeyCode.C))
			{
				yMove -= 1;
			}

			if (UnityEngine.Input.GetKey(KeyCode.LeftShift))
			{
				isRun = true;
			}


			if (UnityEngine.Input.GetMouseButton(0))
			{
				isMouseClick = true;
			}

			if (UnityEngine.Input.GetMouseButtonUp(0))
			{
				isMouseClick = false;
			}

			if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
			{
				if (ServiceManager.TryGetService(out ICursorInputService cursorInstance))
				{
					cursorInstance.ToggleActiveCursor();
					cursorInstance.ToggleLockToScreenCursor();
				}
			}

			Vector3 moveDirection = Vector3.zero;

			if (ServiceManager.TryGetService<IPlayerCameraService>(out var cameraService))
			{
				var cameraTransform = cameraService.PlayerCameraTransform;
				Vector3 cameraForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
				Vector3 cameraRight = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;

				moveDirection = (cameraForward * zMove + cameraRight * xMove).normalized;
			}

			foreach (var entity in CursorQuery)
			{
				ref var cursorComponent = ref entity.Get<CursorComponent>();

				if (cursorComponent.IsShowCursor == false)
				{
					mouseXDegree = UnityEngine.Input.GetAxis("Mouse X");
					mouseYDegree = UnityEngine.Input.GetAxis("Mouse Y");
					
				}
			}
			
			foreach (var entity in cameraQuery)
			{
				ref var cameraComponent = ref entity.Get<PlayerCameraComponent>();

				cameraComponent.MouseXDegree = -mouseXDegree;
				cameraComponent.MouseYDegree = mouseYDegree;
			}

			foreach (var entity in inputQuery)
			{
				ref var moveInputComponent = ref entity.Get<InputComponent>();

				moveInputComponent.MoveDirection = moveDirection;

				moveInputComponent.IsMouseClick = isMouseClick;

				moveInputComponent.IsRun = isRun;
			}
		}
	}
}
