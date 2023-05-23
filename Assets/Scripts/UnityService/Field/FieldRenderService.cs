using System;
using System.Collections.Generic;
using BlitzEcs;
using Game;
using Game.Asset;
using Game.Ecs.Component;
using Game.Unit;
using Service.Field;
using UnityEngine;
using View.Ecs.Component;

namespace UnityService.Field
{
	public enum FieldCellType
	{
		Ceil = 0,
		Floor = 1,
		Wall = 2,
	}

	[UnityService(typeof(IFieldRenderService))]
	public class FieldRenderService : MonoBehaviour, IFieldRenderService
	{
		[Serializable]
		private struct FieldCellContainer
		{
			public FieldCellType type;

			public string sourceGuidName;
		}

		private World _world;

		private Query<FieldCellRequestRealizeComponent> _realizeRequests;
		private Query<FieldCellRequestVirtualizeComponent> _virtualizeRequests;

		private Dictionary<FieldCellType, Guid> _cellToGuid = new();

		/// <summary>
		/// EntityId to UnitTemplate
		/// </summary>
		private readonly Dictionary<int, UnitTemplate> _renderedCells = new();

		public void Init(World world)
		{
			_world = world;
			_realizeRequests = new Query<FieldCellRequestRealizeComponent>(world);
			_virtualizeRequests = new Query<FieldCellRequestVirtualizeComponent>(world);

			// FIXME : 테스트용 필드 셀 엔티티 생성 코드
			var cellGuids = new List<FieldCellContainer>()
			{
				new() { type = FieldCellType.Ceil, sourceGuidName = "9db7e44c-cac9-4f8f-b8bf-5b6b975e1c2f" },
				new() { type = FieldCellType.Wall, sourceGuidName = "a6da6ac5-7108-498c-95b2-c1e003f1bbcb" },
				new() { type = FieldCellType.Floor, sourceGuidName = "46bad3e3-6654-463a-b331-666134a2a3ce" },
			};

			foreach (var container in cellGuids)
			{
				if (Guid.TryParse(container.sourceGuidName, out var guid))
				{
					_cellToGuid.Add(container.type, guid);
				}
			}

			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					var entity = world.Spawn();

					var type = ((i + j) % 3) switch
					{
						0 => FieldCellType.Ceil,
						1 => FieldCellType.Wall,
						2 or _ => FieldCellType.Floor,
					};

					entity.Add(new FieldCellComponent());

					entity.Add(new TransformComponent
					{
						Position = new Vector3(i, j, 0),
						Direction = Vector2.zero
					});

					entity.Add(new BoundsComponent
					{
						BoundsSize = Vector3.one,
					});

					if (_cellToGuid.TryGetValue(type, out var guid))
					{
						entity.Add(new UnitComponent
						{
							SourceGuid = guid
						});
					}
				}
			}
		}

		bool IFieldRenderService.IsRendering(Entity entity) => _renderedCells.ContainsKey(entity.Id);

		void IFieldRenderService.Fetch()
		{
			if (AssetFactory.Instance.TryGetAssetReader<UnitPrefabAssetModule>(out var reader) &&
			    reader is UnitPrefabAssetModule unitPrefabAssetModule)
			{
				// 최대한 신규 오브젝트를 덜 생성하기 위해 Despawn 먼저 실행
				_virtualizeRequests.ForEach(((Entity entity, ref FieldCellRequestVirtualizeComponent _) =>
				{
					entity.Remove<FieldCellRequestVirtualizeComponent>();

					if (_renderedCells.TryGetValue(entity.Id, out var unitTemplate))
					{
						// 디스폰 시켜줌
						unitTemplate.Disconnect();
						unitPrefabAssetModule.Despawn(unitTemplate);

						_renderedCells.Remove(entity.Id);
					}
				}));

				_realizeRequests.ForEach(((Entity entity, ref FieldCellRequestRealizeComponent _) =>
				{
					entity.Remove<FieldCellRequestRealizeComponent>();

					if (entity.Has<UnitComponent>() &&
					    entity.Has<TransformComponent>())
					{
						var unitComponent = entity.Get<UnitComponent>();
						var transformComponent = entity.Get<TransformComponent>();

						if (unitPrefabAssetModule.TrySpawn(unitComponent.SourceGuid, transform, out var unitTemplate))
						{
							unitTemplate.Connect(entity);

							_renderedCells.Add(entity.Id, unitTemplate);

							unitTemplate.transform.position = transformComponent.Position;

							// TODO : ECS Component의 값을 이용해 FieldCell의 Sprite 변경
							// 이것 또한 ECS Component로 해결하는 게 좋을 듯?
						}
					}

					// TODO : FieldCell이 아니라 FieldRenderComponent를 만들어서 모든 유닛의 동작을 한 곳에서 제어할 수 있도록...?
				}));
			}
		}
	}
}
