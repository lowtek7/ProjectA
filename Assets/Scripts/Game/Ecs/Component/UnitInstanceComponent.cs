using System;
using Core.Unity;
using Core.Utility;
using UnityEngine;

namespace Game.Ecs.Component
{
	/// <summary>
	/// 항상 source와 instance가 pair로 구성된다는 시나리오에서는 UnitComponent와 통합하는게 좋을지도...?
	/// 그러나 source만 가지고 있는 원본 객체에서 읽어올 base data가 있을 수 있기 때문에
	/// source와 instance가 무조건 pair가 될 시나리오가 성립 할 수 없는 경우가 있다.
	/// 아직은 초창기이기 때문에 그러한 구조를 예측하는 단계.
	/// </summary>
	[Serializable]
	public struct UnitInstanceComponent : IComponent
	{
		[SerializeField]
		private SGuid instanceGuid;

		public Guid InstanceGuid
		{
			get => instanceGuid.Guid;
			set => instanceGuid.Guid = value;
		}

		public IComponent Clone()
		{
			return new UnitInstanceComponent { instanceGuid = instanceGuid };
		}
	}
}
