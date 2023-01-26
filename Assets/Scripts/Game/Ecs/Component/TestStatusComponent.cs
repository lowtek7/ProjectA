using System;
using Core.Unity;

namespace Game.Ecs.Component
{
	/// <summary>
	/// 임시적인 스테이터스 컴포넌트. 테스트용이기 때문에 테스트외 목적으로 사용 금지.
	/// </summary>
	[Serializable]
	public struct TestStatusComponent : IComponent
	{
		public string name;
		
		public int maxHp;

		public int damage;
		
		public IComponent Clone()
		{
			return new TestStatusComponent
			{
				name = name,
				maxHp = maxHp,
				damage = damage
			};
		}
	}
}
