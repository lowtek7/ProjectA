using UnityEngine;

namespace Service.Camera
{
	public interface IPlayerCameraService : IGameService
	{
		void Fetch();
		//구면 좌표계 새로작성 코드
		void SetSpherical(
			(float min, float init, float max) radius,
			(float min, float init, float max) azimuth, 
			(float min, float init, float max) elevation);

		void SetAzimuth(float degree);

		void AddAzimuth(float degree);

		void SetElevation(float degree);

		void AddElevation(float degree);

		void SetRadius(float value);

		void AddRadius(float value);
		
		Vector3 ScreenToWorld(Vector2 screenPos);

		Vector2 WorldToScreen(Vector3 worldPos);

		Vector3 WorldSize { get; }

		Transform PlayerCameraTransform { get; }
	}
}
