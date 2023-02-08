using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.World.Stage.Tilemap
{
	public class TilemapController : MonoBehaviour
	{
		[SerializeField]
		private List<TilemapLayer> tilemapLayers;

		private Dictionary<TileType, UnityEngine.Tilemaps.Tilemap> typeToTilemap = new();

		private void Awake()
		{
			// 그려줄 타일 타입 -> 타일맵 매핑
			foreach (var tilemapLayer in tilemapLayers)
			{
				var tileType = tilemapLayer.TileType;
				var tilemap = tilemapLayer.GetComponent<UnityEngine.Tilemaps.Tilemap>();
				var tilemapRenderer = tilemap.GetComponent<Renderer>();

				var renderInfo = Constants.tileTypeToRenderInfo[tileType];

				tilemap.transform.position = renderInfo.offset;
				tilemapRenderer.sortingOrder = renderInfo.order;

				typeToTilemap.Add(tileType, tilemap);
			}
		}

		public UnityEngine.Tilemaps.Tilemap GetTilemap(TileType type)
		{
			if (typeToTilemap.TryGetValue(type, out var tilemap))
			{
				return tilemap;
			}

			return null;
		}
	}
}
