using System;
using Core.Pool;
using LiteNetLib;
using Service.Network;

namespace Network.NetCommand
{
	public abstract class PooledNetCommand<T> : INetCommand, IDisposable
		where T : class, new()
	{
		private static ObjectPool<T> pool = new ObjectPool<T>();

		protected static T GetOrCreate()
		{
			return pool.Create();
		}

		public virtual void Dispose()
		{
			pool.Return(this as T);
		}

		public abstract short Opcode { get; }
	}
}
