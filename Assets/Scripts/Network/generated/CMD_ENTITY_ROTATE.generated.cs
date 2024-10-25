// generated by RAMG Packet Generator
using MemoryPack;
using Network.NetCommand;

namespace RAMG.Packets
{
	[MemoryPackable]
	public partial class CMD_ENTITY_ROTATE : PooledNetCommand<CMD_ENTITY_ROTATE>
	{
		public int Id { get; set; }
		public uint Time { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
		public float W { get; set; }
		public static CMD_ENTITY_ROTATE Create()
		{
			return GetOrCreate();
		}

		public override short Opcode => (short) RAMG.Packets.Opcode.CMD_ENTITY_ROTATE;
	}
}