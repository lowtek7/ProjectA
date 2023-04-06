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

		public Vector3 MoveDirection
		{
			get => moveDirection;
			set => moveDirection = value;
		}

		public IComponent Clone()
		{
			return new InputComponent
			{
				moveDirection = moveDirection
			};
		}
	}
}
