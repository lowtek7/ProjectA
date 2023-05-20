using System;
using System.Collections.Generic;
using BlitzEcs;
using Game;
using Game.Asset;
using Game.Ecs.Component;
using Game.Unit;
using Library.JSPool;
using Service.Field;
using UnityEngine;
using View.Ecs.Component;

namespace UnityService.Field
{
	public class FieldRenderService : MonoBehaviour, IFieldRenderService
	{
		[SerializeField]
		private string renderCellSourceGuid;

		private Guid _renderCellSourceGuid;

		private World _world;

		private Query<FieldCellRequestRealizeComponent> _realizeRequests;
		private Query<FieldCellRequestVirtualizeComponent> _virtualizeRequests;

		/// <summary>
		/// EntityId to UnitTemplate
		/// </summary>
		private readonly Dictionary<int, UnitTemplate> _renderedCells = new();

		public void Init(World world)
		{
			_world = world;

			_renderCellSourceGuid = new Guid(renderCellSourceGuid);

			_realizeRequests = new Query<FieldCellRequestRealizeComponent>(world);
			_virtualizeRequests = new Query<FieldCellRequestVirtualizeComponent>(world);
		}

		void IFieldRenderService.Fetch()
		{
			_virtualizeRequests.ForEach(((Entity entity, ref FieldCellRequestVirtualizeComponent _) =>
			{
				entity.Remove<FieldCellRequestVirtualizeComponent>();

				if (_renderedCells.TryGetValue(entity.Id, out var unitTemplate))
				{
					unitTemplate.Disconnect();

					_renderedCells.Remove(entity.Id);
				}
			}));

			_realizeRequests.ForEach(((Entity entity, ref FieldCellRequestRealizeComponent _) =>
			{
				entity.Remove<FieldCellRequestRealizeComponent>();

				if (_renderedCells.ContainsKey(entity.Id))
				{
					Debug.LogWarning($"{ entity } has Realized already.");
					return;
				}

				if (AssetFactory.Instance.TryGetAssetReader<UnitPrefabAssetModule>(out var reader) &&
				    reader is UnitPrefabAssetModule unitPrefabAssetModule)
				{
					if (unitPrefabAssetModule.TrySpawn(_renderCellSourceGuid, transform, out var unitTemplate))
					{
						unitTemplate.Connect(entity);

						_renderedCells.Add(entity.Id, unitTemplate);

						// TODO : ECS Component의 값을 이용해 FieldCell의 Sprite 변경
						// 이것 또한 ECS Component로 해결하는 게 좋을 듯?
					}
				}
			}));
		}
	}
}
