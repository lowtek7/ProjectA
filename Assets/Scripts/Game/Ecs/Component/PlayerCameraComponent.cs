using System;
using Core.Unity;

namespace Game.Ecs.Component
{
	/// <summary>
	/// 플레이어 카메라 컴포넌트는 무조건 하나만 다뤄야 한다.
	/// </summary>
	
	/// <summary>
	/// 구면좌표계를 통해 3인칭 카메라 작성
	/// </summary>

	[Serializable]
	public struct PlayerCameraComponent : IComponent
	{
		private float radius;
		public float minRadius, maxRadius;

		private float azimuthInRad;
		public float minAzimuthInRad, maxAzimuthInRad;           // 방위각

		private float elevationInRad;
		public float minElevationInRad, maxElevationInRad;     // 앙각

		public float Radius
		{
			get => radius;
			set => radius = value;
		}
		
		public float Azimuth
		{
			get => azimuthInRad;
			set => azimuthInRad = value;
		}

		public float Elevation
		{
			get => elevationInRad;
			set => elevationInRad = value;
		}

		public IComponent Clone()
		{
			return new PlayerCameraComponent()
			{
				radius = radius,
				azimuthInRad = azimuthInRad,
				elevationInRad = elevationInRad,
				minRadius = minRadius,
				maxRadius = maxRadius,
				minAzimuthInRad = minAzimuthInRad,
				maxAzimuthInRad = maxAzimuthInRad,
				minElevationInRad = minElevationInRad,
				maxElevationInRad = maxElevationInRad
			};
		}
	}
}
