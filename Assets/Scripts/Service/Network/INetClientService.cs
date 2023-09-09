namespace Service.Network
{
	public interface INetCommand
	{
		short Opcode { get; }
	}

	/// <summary>
	/// 주기적 패킷 타입
	/// </summary>
	public enum PeriodPacketType
	{
		None,
		HalfSecond = 1,
		OneSecond,
	}

	public interface INetClientService : IGameService
	{
		public int NetId { get; }

		void SendCommand(INetCommand command);

		void SendPeriodCommand(PeriodPacketType periodPacketType, INetCommand command);

		void SendPeriodCommand(string key, PeriodPacketType periodPacketType, INetCommand command);
	}
}
