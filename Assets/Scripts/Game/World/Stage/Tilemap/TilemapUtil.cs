using UnityEngine.Tilemaps;

namespace Game.World.Stage.Tilemap
{
	public static class TilemapUtil
	{
		public static int GetTileDirection(string name)
		{
			var splitStrs = name.Split('_');
			var dirStr = splitStrs[^1];

			if (int.TryParse(dirStr, out var dir))
			{
				return dir;
			}

			return -1;
		}

		public static TileType GetTileTypeFromName(string name)
		{
			if (name.StartsWith("ceil"))
			{
				return TileType.Ceil;
			}
			else if (name.StartsWith("side_low"))
			{
				return TileType.SideLow;
			}
			else if (name.StartsWith("side_high"))
			{
				return TileType.SideHigh;
			}
			else if (name.StartsWith("floor"))
			{
				return TileType.Floor;
			}

			return TileType.None;
		}

		public static string GetTileGroupFromName(string name)
		{
			// FIXME : 임시
			if (name.StartsWith("ceil"))
			{
				return "wall_default";
			}
			else if (name.StartsWith("side_low"))
			{
				return "wall_default";
			}
			else if (name.StartsWith("side_high"))
			{
				return "wall_default";
			}
			else
			{
				return name;
			}
		}
	}
}
