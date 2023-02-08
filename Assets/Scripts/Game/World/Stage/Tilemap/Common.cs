using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.World.Stage.Tilemap
{
	public enum TileType
	{
		Floor,
		SideLow,
		SideHigh,
		Ceil,
		None,
	}

	public struct TilemapRenderInfo
	{
		public int order;
		public Vector3 offset;

		public TilemapRenderInfo(int order, int yOffset)
		{
			this.order = order;

			// FIXME
			offset = new Vector3(0, yOffset, 0);
		}
	}

	public static class Constants
	{
		// FIXME : 이걸 내부에서 정해주지 않고 소팅 오더 및 레이어 관련 데이터 파일을 만들어야 할 듯
		public static readonly Dictionary<TileType, TilemapRenderInfo> tileTypeToRenderInfo = new()
		{
			[TileType.Floor] = new(0, 0),
			[TileType.SideLow] = new(1, 0),
			[TileType.SideHigh] = new(2, 1),
			[TileType.Ceil] = new(3, 2),
		};
	}
}
