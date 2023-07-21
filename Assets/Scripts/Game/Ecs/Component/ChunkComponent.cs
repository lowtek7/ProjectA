using System;
using Core.Unity;

namespace Game.Ecs.Component
{
	[Serializable]
	public struct ChunkComponent : IComponent
	{
		public int coordId;

		public ushort[] blockIdMap;

		/// <summary>
		/// FIXME : 블럭 자체의 고체 여부 판단이 아니라, 맞닿은 면의 특성을 가지고 판단해야 함
		/// </summary>
		public bool[] isSolidMap;

		public bool isEmpty;

		public IComponent Clone()
		{
			var newBlockIdMap = new ushort[blockIdMap.Length];
			blockIdMap.CopyTo(newBlockIdMap, blockIdMap.Length);

			var newIsSolidMap = new bool[isSolidMap.Length];
			isSolidMap.CopyTo(newIsSolidMap, isSolidMap.Length);

			return new ChunkComponent
			{
				coordId = coordId,
				blockIdMap = newBlockIdMap,
				isSolidMap = newIsSolidMap,
				isEmpty = isEmpty,
			};
		}
	}
}
