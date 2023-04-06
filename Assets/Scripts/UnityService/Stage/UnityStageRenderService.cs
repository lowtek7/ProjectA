using System;
using System.Collections;
using System.Collections.Generic;
using BlitzEcs;
using Game;
using Game.Asset;
using Game.Ecs.Component;
using Game.Unit;
using Service;
using UnityEngine;
using View.Service;

namespace UnityService.Stage
{
	public class UnityStageRenderService : MonoBehaviour, IStageRenderService
	{
		private World selfWorld = null;

		private bool isLoading = false;

		public bool IsLoading => isLoading;

		/// <summary>
		/// Key = Entity Id
		/// Value = Unit Template
		/// </summary>
		private readonly Dictionary<int, UnitTemplate> loadedUnits = new ();

		/// <summary>
		/// 현재 스테이지에서 해당 엔티티 Id가 그려지고 있는지 검사
		/// 함수 이름은 바꿀 필요가 있는듯
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public bool Contains(int entityId) => loadedUnits.ContainsKey(entityId);

		private void Awake()
		{
			// 일단 임시적으로 여기서 등록해주자...
			if (Application.isPlaying)
			{
				ServiceManager.SetService(typeof(IStageRenderService), this);
			}
		}

		private void OnDestroy()
		{
			// 임시적으로 여기서 제거한다.
			if (Application.isPlaying)
			{
				ServiceManager.ClearService(typeof(IStageRenderService));
			}
		}

		public void Init(World world)
		{
			selfWorld = world;
		}

		/// <summary>
		/// 특정한 스테이지로 이동하는 함수
		/// </summary>
		public void StageTransition(Guid stageGuid)
		{
			// 여기서 로드 되어있는 스테이지는 언로드하고 새로운 스테이지는 로드
			if (!isLoading)
			{
				StartCoroutine(StageTransitionProcess(stageGuid));
			}
			else
			{
				Debug.LogError("StageTransition Failed.");
			}
		}

		private IEnumerator StageTransitionProcess(Guid stageGuid)
		{
			isLoading = true;
			var assetFactory = AssetFactory.Instance;
			if (assetFactory.TryGetAssetReader<UnitPrefabAssetModule>(out var reader) &&
				reader is UnitPrefabAssetModule unitPrefabAssetModule)
			{
				// UI적으로 페이드 먹이거나 하는 것도 여기서 해줘야 함. (검은 화면으로 로딩하는 모습을 가리기 위해)
				foreach (var keyValuePair in loadedUnits)
				{
					StageExitEventInternal(unitPrefabAssetModule, keyValuePair.Value);
				}

				loadedUnits.Clear();
				yield return null;

				// ZoneComponent를 지닌 엔티티들을 가져와야한다.
				var query = new Query<UnitComponent, ZoneComponent>(selfWorld);
				query.Fetch();
				query.ForEach((Entity entity, ref UnitComponent unitComponent, ref ZoneComponent zoneComponent) =>
				{
					// 해당 엔티티가 속한 StageId가 이동하려는 곳과 같은 경우 불러와 준다.
					if (zoneComponent.StageGuid == stageGuid)
					{
						if (StageEnterEventInternal(unitPrefabAssetModule, unitComponent.SourceGuid, entity, out var unitTemplate))
						{
							loadedUnits.Add(entity.Id, unitTemplate);
						}
					}
				});
			}
			else
			{
				Debug.LogError("StageTransitionProcess Failed! UnitPrefabAssetModule is unloaded");
			}

			yield return null;
			isLoading = false;
		}

		private bool StageEnterEventInternal(UnitPrefabAssetModule unitPrefabAssetModule, Guid sourceGuid, Entity entity, out UnitTemplate unitTemplate)
		{
			unitTemplate = null;
			if (unitPrefabAssetModule.TrySpawn(sourceGuid, out var result))
			{
				unitTemplate = result;
				result.Connect(entity);
				return true;
			}
			
			return false;
		}

		/// <summary>
		/// 스테이지 도중에 특정 엔티티가 입장 할 경우 이 함수를 호출 해준다.
		/// </summary>
		/// <param name="entity"></param>
		public void StageEnterEvent(Entity entity)
		{
			if (isLoading)
			{
				Debug.LogWarning("StageEnterEvent Failed. StageRenderService is loading...");
				return;
			}

			var entityId = entity.Id;

			if (loadedUnits.ContainsKey(entityId))
			{
				Debug.LogWarning("Entity is already loadedUnits");
				return;
			}

			if (!entity.Has<UnitComponent>())
			{
				Debug.LogError("Entity does not have UnitComponent.");
				return;
			}

			var assetFactory = AssetFactory.Instance;
			if (assetFactory.TryGetAssetReader<UnitPrefabAssetModule>(out var reader) &&
				reader is UnitPrefabAssetModule unitPrefabAssetModule)
			{
				var unitComponent = entity.Get<UnitComponent>();
				if (StageEnterEventInternal(unitPrefabAssetModule,
						unitComponent.SourceGuid,
						entity,
						out var unitTemplate))
				{
					loadedUnits.Add(entityId, unitTemplate);
				}
			}
		}

		private void StageExitEventInternal(UnitPrefabAssetModule unitPrefabAssetModule, UnitTemplate unitTemplate)
		{
			// 무조건 Disconnect를 선행으로 작업해주어야 한다.
			unitTemplate.Disconnect();
			unitPrefabAssetModule.Despawn(unitTemplate);
		}

		/// <summary>
		/// 스테이지 도중에 엔티티가 사라질 경우 이 함수를 호출 해준다.
		/// </summary>
		/// <param name="entity"></param>
		public void StageExitEvent(Entity entity)
		{
			if (isLoading)
			{
				Debug.LogWarning("StageExitEvent Failed. StageRenderService is loading...");
				return;
			}

			var entityId = entity.Id;

			if (loadedUnits.TryGetValue(entityId, out var unitTemplate))
			{
				var assetFactory = AssetFactory.Instance;
				if (assetFactory.TryGetAssetReader<UnitPrefabAssetModule>(out var reader) &&
					reader is UnitPrefabAssetModule unitPrefabAssetModule)
				{
					StageExitEventInternal(unitPrefabAssetModule, unitTemplate);
					loadedUnits.Remove(entityId);
				}
			}
		}
	}
}
