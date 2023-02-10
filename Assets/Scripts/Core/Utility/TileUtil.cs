using System;
using UnityEngine.Tilemaps;

namespace Core.Utility
{
	public enum TileType
	{
		None = 0,
		Ground = 1 << 0,
		GroundDecal = 1 << 1,
		SideLower = 1 << 2,
		SideUpper = 1 << 3,
		SideDecal = 1 << 4,
		Ceil = 1 << 5,
		ZGroupable = Ceil | SideLower | SideUpper,
		XyGroupable = Ground | Ceil,
	}

	public static class TileUtil
	{
		public static Direction GetDirection(string fullTileName)
		{
			var splitFull = fullTileName.Split('-');
			var split = splitFull[1].Split('_');
			var dirStr = split[^1];

			// 숫자가 범위를 넘어갔을 때에도 None 반환
			if (!Int32.TryParse(dirStr, out var dirInteger) ||
			    dirInteger < 0 || dirInteger > (int) Direction.All)
			{
				return Direction.None;
			}

			return (Direction) dirInteger;
		}

		public static string GetGroup(string fullTileName)
		{
			// 방향 데이터가 없는 경우 타입 데이터만 잘라내고 뒷 스트링을 통째로 취함
			if (GetDirection(fullTileName) == Direction.None)
			{
				return fullTileName.Split('-')[1];
			}

			var splitFull = fullTileName.Split('-');
			var tileName = splitFull[1];

			// 방향 정보는 제외하고 그룹 이름만 남김
			return tileName.Substring(0, tileName.LastIndexOf('_'));
		}

		public static TileType GetTileType(string fullTileName)
		{
			var splitFull = fullTileName.Split('-');

			var typeName = splitFull[0];

			return typeName switch
			{
				"ground" => TileType.Ground,
				"ground_decal" => TileType.GroundDecal,
				"side_lower" => TileType.SideLower,
				"side_upper" => TileType.SideUpper,
				"side_decal" => TileType.SideDecal,
				"ceil" => TileType.Ceil,
				_ => TileType.None
			};
		}
	}
}
