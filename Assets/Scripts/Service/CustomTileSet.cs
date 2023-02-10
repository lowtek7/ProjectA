using System;
using System.Collections.Generic;
using Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Game.Service
{
	[Serializable]
	public struct TileInfo
	{
		public int id;
		public TileBase tile;
		public TileType type;
		public string group;
		public Direction connectDir;
	}

	[Serializable]
	public struct TileXyGroupData
	{
		public TileType type;
		public string group;
		public List<int> tileIds;
	}

	[Serializable]
	public struct TileZGroupData
	{
		public string group;
		public Direction connectDir;
		public List<int> tileIds;
	}

	[CreateAssetMenu(fileName = "TileSet", menuName = "Stage/TileSet")]
	public class CustomTileSet : ScriptableObject
	{
		[SerializeField]
		private List<TileBase> tiles;

		[SerializeField]
		private List<TileInfo> tileInfos;

		[SerializeField]
		private List<TileXyGroupData> xyGroup;

		[SerializeField]
		private List<TileZGroupData> zGroup;

		public List<TileBase> Tiles => tiles;
		public List<TileInfo> TileInfos => tileInfos;
		public List<TileXyGroupData> XyGroup => xyGroup;
		public List<TileZGroupData> ZGroup => zGroup;
	}
}
