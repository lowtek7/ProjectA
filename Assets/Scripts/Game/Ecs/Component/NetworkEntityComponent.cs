using System;
using Core.Unity;

namespace Game.Ecs.Component
{
	public enum EntityRole
	{
		Local = 0,
		Remote
	}

	/// <summary>
	/// 네트워크 상의 엔티티의 Id 정보 컴포넌트
	/// 절대로 복사 되거나 저장되면 안된다.
	/// </summary>
	public struct NetworkEntityComponent
	{
		public EntityRole EntityRole { get; set; }

		public int NetId { get; set; }
	}
}
