using Core.Unity;
using UnityEngine;

namespace Game.Ecs.Component
{
	/// <summary>
	/// 네트워크 엔티티들을 위한 네트워크 무브먼트 컴포넌트
	/// </summary>
	public struct NetMovementComponent : IComponent
	{
		public Vector3? TargetPos { get; set; }

		public Vector3 Velocity { get; set; }

		public Quaternion GoalRotation { get; set; }

		/// <summary>
		/// 캐릭터가 움직이는 여부.
		/// moving이 false이면 순간이동 판정.
		/// </summary>
		public bool IsMoving { get; set; }

		public bool IsRun { get; set; }

		public IComponent Clone()
		{
			return new NetMovementComponent
			{

			};
		}
	}
}
