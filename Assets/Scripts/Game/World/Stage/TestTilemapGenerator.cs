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
		WallBottom,
		WallTop,
		Ceil
	}

	[Serializable]
	public struct CustomTileInfo
	{
		public TileBase tile;
		public string name;

		public TileType type;

		// FIXME : 같이 혹은 여러 타일을 한 개로 묶어서 그려주는 방식을 데이터화하여 같이 묶어야 함
		// 관련 기능을 Custom Rule Tile로 구현할 수 있는지 확인 필요!!!
		public Vector2Int size;
		public string connectedTileName;
	}

	[Serializable]
	public struct TileLayerInfo
	{
		public Tilemap tilemap;
		public TileType drawTileType;
	}

	// 런타임 Tilemap 생성 매니지먼트 테스트용
	// TODO : 데이터셋 구성은 MonoBehaviour가 아니라 ScriptableObject로 분리
	public class TestTilemapGenerator : MonoBehaviour
	{
		// FIXME : 이걸 내부에서 정해주지 않고 소팅 오더 및 레이어 관련 데이터 파일을 만들어야 할 듯
		public static Dictionary<TileType, int> TileTypeToOrder = new Dictionary<TileType, int>()
		{
			[TileType.Floor] = 0,
			[TileType.WallBottom] = 1,
			[TileType.WallTop] = 2,
			[TileType.Ceil] = 3,
		};

		[SerializeField]
		private List<CustomTileInfo> tiles;

		private Dictionary<string, CustomTileInfo> tileDict;

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
			tileDict = tiles.ToDictionary(keySelector: tile => tile.name);

			for (int i = 0; i < tileLayers.Count; i++)
			{
				var tilemap = tileLayers[i].tilemap;
				
				var renderer = tilemap.GetComponent<Renderer>();

				renderer.sortingOrder = TileTypeToOrder[tileLayers[i].drawTileType];
			}

			tilemapDict = tileLayers.ToDictionary(info => info.drawTileType, info => info.tilemap);

			for (int i = 0; i < map.GetLength(0); i++)
			{
				for (int j = 0; j< map.GetLength(1); j++)
				{
					string tileKey = map[i, j];
					var tileInfo = tileDict[tileKey];

					DrawTile(j, i, tileInfo);
					
					if (!string.IsNullOrEmpty(tileInfo.connectedTileName))
					{
						var connectedTileInfo = tileDict[tileInfo.connectedTileName];

						DrawTile(j, i, connectedTileInfo);
					}
				}
			}
		}

		private void DrawTile(int xIndex, int yIndex, CustomTileInfo info)
		{
			if (tilemapDict.TryGetValue(info.type, out var tilemap))
			{
				var drawPos = new Vector3Int(xIndex, yIndex);

				tilemap.SetTile(drawPos, info.tile);
			}
		}
	}
}
