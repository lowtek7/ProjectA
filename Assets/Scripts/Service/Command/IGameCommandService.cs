namespace Service.Command
{
	public interface IGameCommandService : IGameService
	{
		bool TryGet(int type, out IGameCommand gameCommand);
	}
}
