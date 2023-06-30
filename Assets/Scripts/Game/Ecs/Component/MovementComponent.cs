using System;
using Core.Unity;
using UnityEngine;

namespace Game.Ecs.Component
{
	[Serializable]
	public struct MovementComponent : IComponent
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

		[SerializeField]
		private float rotateSpeed;

		public float RotateSpeed
		{
			get => rotateSpeed;
			set => rotateSpeed = value;
		}

		[SerializeField]
		private Quaternion targetRotation;

		public Quaternion TargetRotation
		{
			get => targetRotation;
			set => targetRotation = value;
		}

		public bool IsMoving => MoveDir != Vector3.zero;

		public IComponent Clone()
		{
			return new MovementComponent
			{
				moveDir = moveDir,
				moveSpeed = moveSpeed,
				rotateSpeed = rotateSpeed,
				targetRotation = targetRotation
			};
		}
	}
}
