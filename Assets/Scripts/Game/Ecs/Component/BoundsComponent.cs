using System;
using Core.Unity;
using UnityEngine;

namespace Game.Ecs.Component
{
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
			return new Bounds(pos + Offset, boundsSize);
		}

		public IComponent Clone()
		{
			return new BoundsComponent();
		}
	}
}
