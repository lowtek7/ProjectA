using System;
using Core.Unity;
using UnityEngine;

namespace Game.Ecs.Component
{
	/// <summary>
	/// Stage Partition 영향을 받는 Bounds Component
	/// </summary>
	[Serializable]
	public struct CapsuleColliderComponent : IComponent
	{
		[SerializeField]
		private Vector3 center;

		public Vector3 Center
		{
			get => center;
			set => center = value;
		}

		[SerializeField]
		private Vector3 direction;

		public Vector3 Direction
		{
			get => direction;
			set => direction = value;
		}

		[SerializeField]
		private float radius;

		public float Radius
		{
			get => radius;
			set => radius = value;
		}

		[SerializeField]
		private float height;

		public float Height
		{
			get => height;
			set => height = value;
		}

		public IComponent Clone()
		{
			return new CapsuleColliderComponent
			{
				center = center,
				direction = direction,
				radius = radius,
				height = height
			};
		}
	}
}
