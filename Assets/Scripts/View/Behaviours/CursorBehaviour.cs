using System;
using Service;
using Service.Camera;
using UnityEngine;
using View.Manager;

namespace View.Behaviours
{
	/// <summary>
	/// 커서 객체의 비헤이비어
	/// </summary>
	public class CursorBehaviour : MonoBehaviour, ICursor
	{
		private Vector2 screenPos;
		private Vector3 worldPos;
		
		private void Awake()
		{
			Cursor.visible = false;
			screenPos = Vector2.zero;
			worldPos = Vector3.zero;
			CursorManager.SetCursor(this);
		}

		private void OnDestroy()
		{
			CursorManager.ClearCursor();
			Cursor.visible = true;
		}

		private void Update()
		{
			screenPos = Input.mousePosition;
			worldPos = Vector3.zero;

			if (ServiceManager.TryGetService(out IPlayerCameraService cameraService))
			{
				worldPos = cameraService.ScreenToWorld(screenPos);
				worldPos.z = 0;
			}

			transform.position = worldPos;
		}

		public Vector2 ScreenPos => screenPos;
		public Vector3 WorldPos => worldPos;
	}
}
