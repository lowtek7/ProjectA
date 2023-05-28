namespace Service.World
{
	/// <summary>
	/// 월드를 가져오는 서비스.
	/// 디버깅을 위해서 만들게 되었음.
	/// 별로 좋은 구조는 아니지만 대체할 구조가 생긴다면 다른 형식으로 대체 할 것...
	/// </summary>
	public interface IWorldGetterService : IGameService
	{
		BlitzEcs.World World { get; }
	}
}
