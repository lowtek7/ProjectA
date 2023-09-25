using System;
using Core.Unity;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Ecs.Component
{
	[Serializable]
	public struct InputComponent : IComponent
	{
		[SerializeField]
		private Vector3 moveDirection;

		[SerializeField]
		private Vector2 mouseXYDegree;
		
		[SerializeField]
		private bool changeCameraState;

		[SerializeField]
		private bool isMouseClick;

		[SerializeField]
		private bool isRun;

		public Vector3 MoveDirection
		{
			get => moveDirection;
			set => moveDirection = value;
		}

		public Vector2 MouseXYDegree
		{
			get => mouseXYDegree;

			set => mouseXYDegree = value;
		}

		public bool IsMouseClick
		{
			get => isMouseClick;

			set => isMouseClick = value;
		}

		public bool IsRun
		{
			get => isRun;
			set => isRun = value;
		}

		public IComponent Clone()
		{
			return new InputComponent
			{
				moveDirection = moveDirection,
				mouseXYDegree = mouseXYDegree,
				isMouseClick= isMouseClick,
				isRun = isRun,
			};
		}
	}
}
