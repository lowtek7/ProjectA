using UnityEngine;

namespace Service.Camera
{
	public interface IPlayerCameraService : IGameService
	{
		void SetCameraPosition(Vector2 position);

		Vector3 ScreenToWorld(Vector2 screenPos);

		Vector2 WorldToScreen(Vector3 worldPos);

		Vector2 WorldSize { get; }
	}
}
