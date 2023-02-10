using Core.Unity;

namespace Game.Ecs.Component
{
	/// <summary>
	/// 플레이어 카메라 컴포넌트는 무조건 하나만 다뤄야 한다.
	/// </summary>
	public struct PlayerCameraComponent : IComponent
	{
		public IComponent Clone()
		{
			return new PlayerCameraComponent();
		}
	}
}
