using Core.Unity;

namespace Game.Ecs.Component
{
	/// <summary>
	/// 플레이어 컴포넌트
	/// </summary>
	public struct PlayerComponent : IComponent
	{
		public IComponent Clone()
		{
			return new PlayerComponent();
		}
	}
}
