using MemoryPack;

namespace Network.NetCommand.Client.Entity
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

		public void SetRotate(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public override short Opcode => (short) Network.Packet.Opcode.CMD_ENTITY_ROTATE;
	}
}
