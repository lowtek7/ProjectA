using System;
using Core.Unity;
using Core.Utility;
using UnityEngine;

namespace View.Ecs.Component
{
	[Serializable]
	public struct StageRenderComponent : IComponent
	{
		/// <summary>
		/// 현재 렌더링 중인 스테이지의 ID
		/// </summary>
		[SerializeField]
		private SGuid stageGuid;

		public Guid StageGuid
		{
			get => stageGuid.Guid;
			set => stageGuid.Guid = value;
		}

		public IComponent Clone()
		{
			return new StageRenderComponent { stageGuid = stageGuid};
		}
	}
}
