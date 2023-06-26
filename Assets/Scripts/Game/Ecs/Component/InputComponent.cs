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
		private Vector3 cameraRotation;

		[SerializeField]
		private bool isMouseClick;

		public Vector3 MoveDirection
		{
			get => moveDirection;
			set => moveDirection = value;
		}
		
		public Vector2 CameraRotation
		{
			get => cameraRotation;
			
			set => cameraRotation = value;
		}
		
		public bool IsMouseClick
		{
			get => isMouseClick;
			
			set => isMouseClick = value;
		}

		public IComponent Clone()
		{
			return new InputComponent
			{
				moveDirection = moveDirection,
				cameraRotation = cameraRotation,
				isMouseClick= isMouseClick,
			};
		}
	}
}
