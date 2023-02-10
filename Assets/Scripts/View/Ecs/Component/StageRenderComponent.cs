using System;
using Core.Unity;
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
		private int stageId;

		public int StageId
		{
			get => stageId;
			set => stageId = value;
		}

		public IComponent Clone()
		{
			return new StageRenderComponent { stageId = stageId};
		}
	}
}
