using System;
using Core.Unity;
using Core.Utility;
using UnityEngine;

namespace Game.Ecs.Component
{
	/// <summary>
	/// 해당 컴포넌트의 SourceGuid를 이용해서 실제 Prefab 에셋을 찾는데 사용 된다.
	/// </summary>
	[Serializable]
	public struct UnitComponent : IComponent
	{
		[SerializeField]
		private SGuid sourceGuid;

		public Guid SourceGuid
		{
			get => sourceGuid.Guid;
			set => sourceGuid.Guid = value;
		}

		public IComponent Clone()
		{
			return new UnitComponent { sourceGuid = sourceGuid };
		}
	}
}
