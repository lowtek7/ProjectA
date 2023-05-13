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

		public Vector3 BoundsSize
		{
			get => boundsSize;
			set => boundsSize = value;
		}

		public Bounds GetBounds(Vector3 pos)
		{
			return new Bounds(pos, boundsSize);
		}

		public IComponent Clone()
		{
			return new BoundsComponent();
		}
	}
}
