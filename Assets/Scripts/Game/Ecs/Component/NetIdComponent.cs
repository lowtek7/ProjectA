using System;
using Core.Unity;

namespace Game.Ecs.Component
{
	/// <summary>
	/// 네트워크 상의 엔티티의 Id 정보 컴포넌트
	/// 절대로 복사 되거나 저장되면 안된다.
	/// </summary>
	public struct NetIdComponent
	{
		public int NetId { get; set; }
	}
}
