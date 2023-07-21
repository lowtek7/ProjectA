using MemoryPack;
using Service.Network;

namespace Network.Packet.Handler
{
	public interface IPacketHandler
	{
		INetCommand ToCommand(ref MemoryPackReader reader);
	}
}
