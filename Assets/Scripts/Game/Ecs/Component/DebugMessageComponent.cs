using System;
using Core.Unity;

namespace Game.Ecs.Component
{
	[Serializable]
	public struct DebugMessageComponent : IComponent
	{
		public string message;
		
		public IComponent Clone()
		{
			return new DebugMessageComponent
			{
				message = message
			};
		}
	}
}
