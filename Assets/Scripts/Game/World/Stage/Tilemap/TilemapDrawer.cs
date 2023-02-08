using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.World.Stage.Tilemap
{
	public class TileGroup
	{
#if UNITY_EDITOR
		// 디버그용
		private string groupName;
#endif

		private Dictionary<int, TileBase> tiles = new();

#if UNITY_EDITOR
		public TileGroup(string groupName)
		{
			this.groupName = groupName;
		}
#endif
		public void Add(TileBase tile)
		{
			int dirNum = TilemapUtil.GetTileDirection(tile.name);

			tiles.Add(dirNum, tile);
		}

		public TileBase GetTile(int dir = -1)
		{
			if (tiles.TryGetValue(dir, out var tile))
			{
				return tile;
			}

			// null 반환하는 일은 없어야 함!
			Debug.LogError("No Tile Direction Data. Please Check Tile Assets");
			return null;
		}

		public bool TryGetTile(int dir, out TileBase tile)
		{
			return tiles.TryGetValue(dir, out tile);
		}
	}

	// 런타임 Tilemap 생성 매니지먼트 테스트용
	// TODO : 데이터셋 구성은 MonoBehaviour가 아니라 ScriptableObject로 분리
	// 테스트용이므로 지우고 새로 만들 것임
	public class TilemapDrawer
	{
		private Dictionary<TileType, Dictionary<string, TileGroup>> tileTypeGroups = new()
		{
			[TileType.Ceil] = new(),
			[TileType.SideHigh] = new(),
			[TileType.SideLow] = new(),
			[TileType.Floor] = new(),
		};

		private Dictionary<TileBase, string> tileToGroupName = new();

		private TilemapController tilemapController;

		public TilemapDrawer(List<TileBase> tiles, TilemapController tilemapController)
		{
			this.tilemapController = tilemapController;

			foreach (var tile in tiles)
			{
				var tileType = TilemapUtil.GetTileTypeFromName(tile.name);
				var tileGroupName = TilemapUtil.GetTileGroupFromName(tile.name);
				var tileGroup = tileTypeGroups[tileType];

				if (!tileGroup.ContainsKey(tileGroupName))
				{
#if UNITY_EDITOR
					var group = new TileGroup(tileGroupName);
#else
					group = new TileGroup();
#endif
					tileGroup.Add(tileGroupName, group);
				}

				tileGroup[tileGroupName].Add(tile);

				tileToGroupName.Add(tile, tileGroupName);
			}
		}

		public void Draw(StageMap stageMap)
		{
			var map = stageMap.map;

			for (int i = 0; i < map.GetLength(0); i++)
			{
				for (int j = 0; j < map.GetLength(1); j++)
				{
					string tileKey = map[i, j];
					var drawPos = new Vector3Int(j, i);

					// FIXME
					if (tileKey == "wall")
					{
						var tilemap = tilemapController.GetTilemap(TileType.Ceil);

						// FIXME
						var group = tileTypeGroups[TileType.Ceil]["wall_default"];

						tilemap.SetTile(drawPos, group.GetTile());
					}
					else if (tileKey == "floor")
					{
						var tilemap = tilemapController.GetTilemap(TileType.Floor);

						// FIXME
						var group = tileTypeGroups[TileType.Floor]["floor_a"];

						tilemap.SetTile(drawPos, group.GetTile());
					}
				}
			}

			// RuleTile 갱신 대기 후 Side Draw

			var ceilTilemap = tilemapController.GetTilemap(TileType.Ceil);

			for (int i = 0; i < map.GetLength(0); i++)
			{
				for (int j = 0; j < map.GetLength(1); j++)
				{
					var drawPos = new Vector3Int(j, i);

					var ceilTile = ceilTilemap.GetTile<RuleTile>(drawPos);

					if (ceilTile == null)
					{
						continue;
					}

					var tileData = new TileData
					{
						color = Color.white,
						transform = Matrix4x4.identity,
						flags = TileFlags.None,
						colliderType = Tile.ColliderType.None
					};

					ceilTile.GetTileData(drawPos, ceilTilemap, ref tileData);

					var dirNum = TilemapUtil.GetTileDirection(tileData.sprite.name);
					var groupName = tileToGroupName[ceilTile];

					if (tileTypeGroups[TileType.SideLow][groupName].TryGetTile(dirNum, out var sideLowTile))
					{
						var tilemap = tilemapController.GetTilemap(TileType.SideLow);

						tilemap.SetTile(drawPos, sideLowTile);
					}

					if (tileTypeGroups[TileType.SideHigh][groupName].TryGetTile(dirNum, out var sideHighTile))
					{
						var tilemap = tilemapController.GetTilemap(TileType.SideHigh);

						tilemap.SetTile(drawPos, sideHighTile);
					}
				}
			}

		}
	}
}
