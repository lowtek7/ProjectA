using System;
using Core.Unity;
using UnityEngine;

namespace Game.Ecs.Component
{
	public enum LifeType
	{
		Infinite = 0,
		Time = 10,
		Count = 20
	}

	/// <summary>
	/// 수명에 연관된 컴포넌트
	/// </summary>
	[Serializable]
	public struct LifeSpanComponent : IComponent
	{
		[SerializeField]
		private LifeType lifeType;

		[SerializeField]
		private int durationCount;

		[SerializeField]
		private float durationTime;

		public float DurationTime
		{
			get => durationTime;
			set => durationTime = value;
		}

		public int DurationCount
		{
			get => durationCount;
			set => durationCount = value;
		}

		public LifeType LifeType
		{
			get => lifeType;
			set => lifeType = value;
		}

		public IComponent Clone()
		{
			return new LifeSpanComponent
			{
				lifeType = lifeType,
				durationCount = durationCount,
				durationTime = durationTime
			};
		}
	}
}
