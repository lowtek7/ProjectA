using UnityEngine;

namespace Service.Camera
{
	public interface IPlayerCameraService : IGameService
	{
		void SetCameraPosition(Vector3 position);

		void SetCameraRotation(Vector2 rotation);

		void SetMouseClick(bool mouseClick);

		void ToggleShowCursor();

		Vector3 ScreenToWorld(Vector2 screenPos);

		Vector2 WorldToScreen(Vector3 worldPos);

		Vector3 WorldSize { get; }
	}
}
