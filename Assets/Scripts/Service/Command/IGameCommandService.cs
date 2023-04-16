namespace Service.Command
{
	public interface IGameCommandService : IGameService
	{
		bool TryGet(int typeId, out IGameCommand gameCommand);
	}
}
