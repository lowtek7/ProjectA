// generated by RAMG Packet Generator
using System;
using MemoryPack;
using Network.NetCommand;
using Network.Packet.Handler;
using Service.Network;

namespace RAMG.Packets
{
	[PacketOpcode(RAMG.Packets.Opcode.CMD_ENTITY_MOVE)]
	public partial class EntityMovePacketHandler : IPacketHandler
	{
		public INetCommand ToCommand(ref MemoryPackReader reader)
		{
			var command = CMD_ENTITY_MOVE.Create();

			CMD_ENTITY_MOVE.Deserialize(ref reader, ref command);

			return command;
		}
	}
}
