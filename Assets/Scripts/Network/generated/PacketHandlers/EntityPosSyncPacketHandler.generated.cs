// generated by RAMG Packet Generator
using System;
using MemoryPack;
using Network.NetCommand;
using Network.Packet.Handler;
using Service.Network;

namespace RAMG.Packets
{
	[PacketOpcode(RAMG.Packets.Opcode.CMD_ENTITY_POS_SYNC)]
	public partial class EntityPosSyncPacketHandler : IPacketHandler
	{
		public INetCommand ToCommand(ref MemoryPackReader reader)
		{
			var command = CMD_ENTITY_POS_SYNC.Create();

			CMD_ENTITY_POS_SYNC.Deserialize(ref reader, ref command);

			return command;
		}
	}
}