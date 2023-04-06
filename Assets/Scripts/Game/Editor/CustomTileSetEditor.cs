using System;
using System.Collections.Generic;
using Core.Utility;
using Service;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	[CustomEditor(typeof(CustomTileSet))]
	public class CustomTileSetEditor : UnityEditor.Editor
	{
		private CustomTileSet tileSet;

		private void OnEnable()
		{
			tileSet = target as CustomTileSet;
		}

		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Apply"))
			{
				tileSet.TileInfos.Clear();
				tileSet.XyGroup.Clear();
				tileSet.ZGroup.Clear();

				int idWalker = 0;

				var xyGroupDict = new Dictionary<(string group, TileType type), List<int>>();
				var zGroupDict = new Dictionary<(string group, Direction connectDir), List<int>>();

				foreach (var tile in tileSet.Tiles)
				{
					var tileName = tile.name;
					var info = new TileInfo
					{
						id = idWalker,
						tile = tile,
						connectDir = TileUtil.GetDirection(tileName),
						type = TileUtil.GetTileType(tileName),
						group = TileUtil.GetGroup(tileName)
					};

					tileSet.TileInfos.Add(info);

					if ((info.type & TileType.XyGroupable) == info.type)
					{
						var typeGroup = (info.group, info.type);

						if (!xyGroupDict.ContainsKey(typeGroup))
						{
							xyGroupDict.Add(typeGroup, new());
						}

						xyGroupDict[typeGroup].Add(info.id);
					}

					if ((info.type & TileType.ZGroupable) == info.type && info.connectDir != Direction.None)
					{
						var directionGroup = (info.group, info.connectDir);

						if (!zGroupDict.ContainsKey(directionGroup))
						{
							zGroupDict.Add(directionGroup, new());
						}

						zGroupDict[directionGroup].Add(info.id);
					}

					idWalker++;
				}

				foreach (var kv in xyGroupDict)
				{
					tileSet.XyGroup.Add(new TileXyGroupData
					{
						group = kv.Key.group,
						type = kv.Key.type,
						tileIds = kv.Value,
					});
				}

				foreach (var kv in zGroupDict)
				{
					tileSet.ZGroup.Add(new TileZGroupData
					{
						group = kv.Key.group,
						connectDir = kv.Key.connectDir,
						tileIds = kv.Value,
					});
				}

				serializedObject.ApplyModifiedProperties();
			}

			base.OnInspectorGUI();
		}
	}
}
