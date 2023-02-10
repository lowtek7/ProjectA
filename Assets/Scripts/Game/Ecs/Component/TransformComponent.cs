using System;
using Core.Unity;
using UnityEngine;

namespace Game.Ecs.Component
{
	/// <summary>
	/// 위치와 연관된 데이터를 기록하는 컴포넌트
	/// </summary>
	[Serializable]
	public struct TransformComponent : IComponent
	{
		[SerializeField]
		private Vector3 position;

		public Vector3 Position
		{
			get => position;
			set => position = value;
		}

		public IComponent Clone()
		{
			return new TransformComponent { position = position };
		}
	}
}
