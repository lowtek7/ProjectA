using LiteNetLib;
using MemoryPack;

namespace Network.NetCommand.Server.Player
{
	[MemoryPackable]
	public partial class SCMD_PLAYER_LOGIN : PooledNetCommand<SCMD_PLAYER_LOGIN>
	{
		public int Id { get; set; }

		public uint Time { get; set; }

		public static SCMD_PLAYER_LOGIN Create()
		{
			return GetOrCreate();
		}

		public override short Opcode => (short) Packet.Opcode.SCMD_PLAYER_LOGIN;
	}
}
