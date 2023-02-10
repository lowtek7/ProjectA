using System;
using UnityEngine;

namespace Game.Ecs.Component
{
	[Serializable]
	public struct MovementComponent
	{
		[SerializeField]
		private float moveSpeed;

		/// <summary>
		/// 이 값이 Vector.zero가 아니라면 Move하게 될 것
		/// </summary>
		[SerializeField]
		private Vector3 moveDir;

		public Vector3 MoveDir
		{
			get => moveDir;
			set => moveDir = value;
		}

		public float MoveSpeed
		{
			get => moveSpeed;
			set => moveSpeed = value;
		}
	}
}
