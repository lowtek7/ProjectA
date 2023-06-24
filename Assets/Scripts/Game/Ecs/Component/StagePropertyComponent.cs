using System;
using Core.Unity;
using Core.Utility;
using UnityEngine;

namespace Game.Ecs.Component
{
	[Serializable]
	public struct StagePropertyComponent : IComponent
	{
		/// <summary>
		/// 스테이지의 guid
		/// </summary>
		[SerializeField]
		private SGuid stageGuid;

		/// <summary>
		/// 해당 스테이지가 생성이 될 수 있을지.
		/// </summary>
		[SerializeField]
		private bool canGenerate;

		public bool CanGenerate
		{
			get => canGenerate;
			set => canGenerate = value;
		}

		public Guid StageGuid
		{
			get => stageGuid.Guid;
			set => stageGuid = new SGuid(value);
		}

		public IComponent Clone()
		{
			return new StagePropertyComponent
			{
				stageGuid = stageGuid,
				canGenerate = canGenerate
			};
		}
	}
}
