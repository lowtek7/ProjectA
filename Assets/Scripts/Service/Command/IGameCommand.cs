using BlitzEcs;

namespace Service.Command
{
	/// <summary>
	/// GameCommand는 게임 내부의 엔티티의 로직을 추상적이며 확장성있는 형태로 디자인 하기 위해 설계한 시스템이다.
	/// 추후 게임 내부에 로직이 스크립트나 특정 룰을 기반으로 돌아가는 상호작용이 포함된 스크립트가 생성 될 수 있다.
	/// 따라서 그에 맞게 한 단계 추상적인 계층을 만들어두었다.
	/// 엔티티의 행동들이라던가 상호작용들이 GameCommand로 구현 될 것이다. 
	/// </summary>
	public interface IGameCommand
	{
		void Execute(Entity entity);
	}
}
