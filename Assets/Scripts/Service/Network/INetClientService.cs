namespace Service.Network
{
	public interface INetCommand
	{
		short Opcode { get; }
	}

	public interface INetClientService : IGameService
	{
		public int NetId { get; }

		void SendCommand(INetCommand command);
	}
}
