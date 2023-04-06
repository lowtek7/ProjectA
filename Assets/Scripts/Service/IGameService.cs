namespace Game.Service
{
	/// <summary>
	/// 서비스를 구축하지 않았기 때문에 임시적으로 여기서
	/// </summary>
	public interface IGameService
	{
		void Init(BlitzEcs.World world);
	}

	public interface IGameServiceCallback
	{
		void OnActivate();
		
		void OnDeactivate();
	}
}
