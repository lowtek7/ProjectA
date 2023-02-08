using System.Collections.Generic;
using System.Linq;
using Game.Asset;
using Game.World.Stage.Tilemap;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.World.Stage
{
	public class StageController
	{
		private TilemapDrawer tilemapDrawer;
		private StageMap map;

		public StageController()
		{
			List<TileBase> tiles;

			var tilePath = "mine";

			if (!AssetFactory.Instance.TryGetAssetReader<TileAssetModule>(out var assetReader) ||
			    !(assetReader is TileAssetModule tileAssetModule))
			{
				Debug.LogError("Cannot Find TileAssetModule");

				return;
			}

			// 사용할 타일들을 AssetFactory에서 긁어옴
			tiles = tileAssetModule.GetInPath(tilePath).ToList();

			// TODO : 기본 타일맵 프리팹도 갖다놓기
			if (!AssetFactory.Instance.TryGetAsset<TilemapRendererAssetModule, GameObject>
				    ("TilemapTemplate", out var templatePrefab))
			{
				Debug.LogError("Cannot Find TilemapRendererAssetModule");

				return;
			}

			var tilemapTemplate = Object.Instantiate(templatePrefab);

			var tilemapController = tilemapTemplate.GetComponent<TilemapController>();

			tilemapDrawer = new TilemapDrawer(tiles, tilemapController);

			map = new();

			tilemapDrawer.Draw(map);
		}
	}
}
