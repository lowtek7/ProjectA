using System;
using Core.Unity;
using UnityEngine;

namespace Game.Ecs.Component
{
	[Serializable]
	public struct MovementComponent : IComponent
	{
		public enum MoveState
		{
			Stop = 0,
			Walk = 1,
			IsRun = 2
		}

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

		[SerializeField]
		private float walkSpeed;

		public float WalkSpeed
		{
			get => walkSpeed;
			set => walkSpeed = value;
		}

		[SerializeField]
		private float runSpeed;

		public float RunSpeed
		{
			get => runSpeed;
			set => runSpeed = value;
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

		private bool _isRun;

		public bool IsRun
		{
			get => _isRun;
			set => _isRun = value;
		}

		public float CurrentSpeed
		{
			get
			{
				if (CurrentMoveState == MoveState.Walk)
				{
					return walkSpeed;
				}

				if (CurrentMoveState == MoveState.IsRun)
				{
					return runSpeed;
				}

				return 0;
			}
		}

		public MoveState CurrentMoveState
		{
			get
			{
				if (MoveDir != Vector3.zero)
				{
					return _isRun == false ? MoveState.Walk : MoveState.IsRun;
				}

				return MoveState.Stop;
			}
		}

		public bool IsMoving => CurrentMoveState != MoveState.Stop;

		public bool IsRunning => CurrentMoveState == MoveState.IsRun;

		public IComponent Clone()
		{
			return new MovementComponent
			{
				moveDir = moveDir,
				walkSpeed = walkSpeed,
				rotateSpeed = rotateSpeed,
				targetRotation = targetRotation,
				_isRun = _isRun
			};
		}
	}
}
