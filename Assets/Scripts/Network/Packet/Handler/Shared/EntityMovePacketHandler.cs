using MemoryPack;
using Network.NetCommand.Client.Entity;
using Service.Network;

namespace Network.Packet.Handler.Shared
{
	[PacketOpcode(Opcode.CMD_ENTITY_MOVE)]
	public class EntityMovePacketHandler : IPacketHandler
	{
		public INetCommand ToCommand(ref MemoryPackReader reader)
		{
			// 패킷을 풀로 부터 가져온다
			var command = CMD_ENTITY_MOVE.Create();

			// 디시리얼라이즈
			CMD_ENTITY_MOVE.Deserialize(ref reader, ref command);

			// 디시리얼라이즈 한 패킷을 반환한다. 사용이 완료된 커맨드는 무조건 dispose로 pool에 반환 해야한다.
			return command;
		}
	}
}