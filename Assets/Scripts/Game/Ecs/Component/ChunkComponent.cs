using System.Collections.Generic;
using Core.Unity;
using UnityEngine;

namespace Game.Ecs.Component
{
	public struct QuadCell
	{
		public ushort textureId;
		public int? entityId;
	}

	public struct Chunk
	{
		public QuadCell[,] cells;
		public bool IsRealized;
	}

	// FIXME : 컴포넌트가 아니라 서비스 데이터로 빼야 하나...?
	public struct ChunkComponent : IComponent
	{
		public Chunk[,] totalChunks;

		public IComponent Clone()
		{
			return new ChunkComponent
			{
				totalChunks = totalChunks
			};
		}
	}
}
