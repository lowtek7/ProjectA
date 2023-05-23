using System;
using Core.Unity;
using UnityEngine;

namespace View.Ecs.Component
{
	/// <summary>
	/// 화면과 해당 바운드가 겹쳐지면 실제 GameObject로 변경
	/// </summary>
	[Serializable]
	public struct FieldRenderBoundsComponent : IComponent
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
			return new FieldRenderBoundsComponent
			{
				boundsSize = boundsSize,
			};
		}
	}
}
