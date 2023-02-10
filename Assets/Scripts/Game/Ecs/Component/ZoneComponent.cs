using System;
using UnityEngine;

namespace Game.Ecs.Component
{
	/// <summary>
	/// 스테이지 Id를 가지고 있는 컴포넌트
	/// 해당 엔티티가 월드 내의 어떤 스테이지에 있는지에 대한 정보를 기록한 컴포넌트다
	/// </summary>
	[Serializable]
	public struct ZoneComponent
	{
		[SerializeField]
		private int stageId;

		public int StageId
		{
			get => stageId;
			set => stageId = value;
		}
	}
}
