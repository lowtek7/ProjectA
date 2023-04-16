using System;
using Core.Unity;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Ecs.Component
{
	/// <summary>
	/// 무엇인가 실행하게 되었을때 부착되는 컴포넌트
	/// type : 커맨드의 타입
	/// magnitude : 커맨드의 강도(Power)나 크기(Size)와 같은 수치
	/// </summary>
	[Serializable]
	public struct CommandComponent : IComponent
	{
		[SerializeField]
		private int typeId;

		[SerializeField]
		private float magnitude;

		public float Magnitude
		{
			get => magnitude;
			set => magnitude = value;
		}

		public int TypeId
		{
			get => typeId;
			set => typeId = value;
		}

		public IComponent Clone()
		{
			return new CommandComponent
			{
				typeId = typeId,
				magnitude = magnitude
			};
		}
	}
}
