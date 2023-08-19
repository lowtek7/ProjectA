using MemoryPack;
using Network.NetCommand.Client.Entity;
using Service.Network;

namespace Network.Packet.Handler.Shared
{
	[PacketOpcode(Opcode.CMD_ENTITY_ROTATE)]
	public class EntityRotatePacketHandler : IPacketHandler
	{
		public INetCommand ToCommand(ref MemoryPackReader reader)
		{
			var command = CMD_ENTITY_ROTATE.Create();

			CMD_ENTITY_ROTATE.Deserialize(ref reader, ref command);

			return command;
		}
	}
}
