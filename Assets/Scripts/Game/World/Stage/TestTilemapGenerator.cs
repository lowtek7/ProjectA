using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.World.Stage
{
	public enum TileType
	{
		Floor,
		SideLow,
		SideHigh,
		Ceil,
		None,
	}

	public struct CustomTileInfo
	{
		public TileBase tile;
		public TileType type;

		public CustomTileInfo(TileBase _tile)
		{
			tile = _tile;

			var tile_name = tile.name;

			if (tile_name.StartsWith("ceil"))
			{
				type = TileType.Ceil;
			}
			else if (tile_name.StartsWith("side_low"))
			{
				type = TileType.SideLow;
			}
			else if (tile_name.StartsWith("side_high"))
			{
				type = TileType.SideHigh;
			}
			else if (tile_name.StartsWith("floor"))
			{
				type = TileType.Floor;
			}
			else
			{
				type = TileType.None;
			}
		}
	}

	[Serializable]
	public struct TileLayerInfo
	{
		public Tilemap tilemap;
		public TileType drawTileType;
	}

	// 런타임 Tilemap 생성 매니지먼트 테스트용
	// TODO : 데이터셋 구성은 MonoBehaviour가 아니라 ScriptableObject로 분리
	// 테스트용이므로 지우고 새로 만들 것임
	public class TestTilemapGenerator : MonoBehaviour
	{
		// FIXME : 이걸 내부에서 정해주지 않고 소팅 오더 및 레이어 관련 데이터 파일을 만들어야 할 듯
		public static Dictionary<TileType, int> TileTypeToOrder = new()
		{
			[TileType.Floor] = 0,
			[TileType.SideLow] = 1,
			[TileType.SideHigh] = 2,
			[TileType.Ceil] = 3,
		};

		[SerializeField]
		private List<TileBase> tiles;

		private Dictionary<string, CustomTileInfo> tileInfoDict = new();

		[SerializeField]
		private List<TileLayerInfo> tileLayers;

		private Dictionary<TileType, Tilemap> tilemapDict;

		// TODO : 데이터를 SerializeField로 혹은 AssetLoader 통해서 읽어오도록 수정
		// 추후 랜덤 생성 기능이 들어갔을 때에는 비워두면 랜덤, 넣어주면 테스트용 정적 맵 로드하도록 할 예정
		private string[,] map =
		{
			{ "wall", "wall", "wall", "wall" },
			{ "wall", "floor", "floor", "wall" },
			{ "wall", "floor", "floor", "wall" },
			{ "wall", "floor", "floor", "wall" },
			{ "wall", "wall", "wall", "wall" },
		};

		private void Awake()
		{
			for (int i = 0; i < tiles.Count; i++)
			{
				var tile = tiles[i];
				tileInfoDict.Add(tile.name, new CustomTileInfo(tile));
			}

			for (int i = 0; i < tileLayers.Count; i++)
			{
				var tilemap = tileLayers[i].tilemap;
				
				var renderer = tilemap.GetComponent<Renderer>();
				var order = TileTypeToOrder[tileLayers[i].drawTileType];

				renderer.sortingOrder = order;
				tilemap.transform.localPosition = new Vector3(0, order, 0);
			}

			tilemapDict = tileLayers.ToDictionary(info => info.drawTileType, info => info.tilemap);

			for (int i = 0; i < map.GetLength(0); i++)
			{
				for (int j = 0; j < map.GetLength(1); j++)
				{
					string tileKey = map[i, j];

					if (tileKey == "wall")
					{
						var tileInfo = tileInfoDict["ceil"];
						var tilemap = tilemapDict[tileInfo.type];

						var drawPos = new Vector3Int(j, i);

						tilemap.SetTile(drawPos, tileInfo.tile);

						var tileData = new TileData
						{
							color = Color.white,
							transform = Matrix4x4.identity,
							flags = TileFlags.None,
							colliderType = Tile.ColliderType.None
						};
						
						tileInfo.tile.GetTileData(drawPos, tilemap, ref tileData);
						var splitStrs = tileData.sprite.name.Split('_');
						var dirStr = splitStrs[^1];

						{
							var sideHighName = $"side_high_{dirStr}";

							if (tileInfoDict.TryGetValue(sideHighName, out var sideHighTileInfo))
							{
								var sideHighTilemap = tilemapDict[TileType.SideHigh];

								sideHighTilemap.SetTile(drawPos, sideHighTileInfo.tile);
							}
						}

						{
							var sideLowName = $"side_low_{dirStr}";

							if (tileInfoDict.TryGetValue(sideLowName, out var sideLowTileInfo))
							{
								var sideLowTilemap = tilemapDict[TileType.SideLow];

								sideLowTilemap.SetTile(drawPos, sideLowTileInfo.tile);
							}
						}
					}
					else
					{
						var tileInfo = tileInfoDict["floor_default_0"];
						var tilemap = tilemapDict[tileInfo.type];

						var drawPos = new Vector3Int(j, i);

						tilemap.SetTile(drawPos, tileInfo.tile);
					}
				}
			}
		}
	}
}
