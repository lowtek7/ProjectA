using System;
using Core.Unity;

namespace Game.Ecs.Component
{
	/// <summary>
	/// 마우스 커서 컴포넌트
	/// </summary>
	[Serializable]
	
	public struct CursorComponent : IComponent
	{
		public IComponent Clone()
		{
			return new InputComponent();
		}
	}
}
