using System;
using Core.Unity;
using Core.Utility;
using UnityEngine;

namespace Game.Ecs.Component
{
	/// <summary>
	/// 기존에 ZoneComponent이었으나 이름이 적절하지 않다고 판단하여 StageSpecComponent로 이름 변경.
	/// 스테이지 Id를 가지고 있는 컴포넌트
	/// 해당 엔티티가 월드 내의 어떤 스테이지에 있는지에 대한 정보를 기록한 컴포넌트다
	/// </summary>
	[Serializable]
	public struct StageSpecComponent : IComponent
	{
		[SerializeField]
		private SGuid stageGuid;

		public Guid StageGuid
		{
			get => stageGuid.Guid;
			set => stageGuid.Guid = value;
		}

		public IComponent Clone()
		{
			return new StageSpecComponent()
			{
				stageGuid = stageGuid
			};
		}
	}
}
