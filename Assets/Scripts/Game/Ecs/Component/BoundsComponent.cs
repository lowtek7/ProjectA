using System;
using Core.Unity;
using UnityEngine;

namespace Game.Ecs.Component
{
	/// <summary>
	/// Stage Partition 영향을 받는 Bounds Component
	/// </summary>
	[Serializable]
	public struct BoundsComponent : IComponent
	{
		[SerializeField]
		private Vector3 boundsSize;

		[SerializeField]
		private Vector3 offset;

		public Vector3 BoundsSize
		{
			get => boundsSize;
			set => boundsSize = value;
		}

		public Vector3 Offset
		{
			get => offset;
			set => offset = value;
		}

		public Bounds GetBounds(Vector3 pos)
		{
			return new Bounds(pos + offset, boundsSize);
		}

		public IComponent Clone()
		{
			return new BoundsComponent
			{
				boundsSize = boundsSize,
				offset = offset
			};
		}
	}
}
