using System;
using Core.Unity;

namespace Game.Ecs.Component
{
	[Serializable]
	public struct GravityComponent : IComponent
	{
		public IComponent Clone()
		{
			return new GravityComponent();
		}
	}
}
