using System;
using System.Collections.Generic;
using BlitzEcs;
using Game.Ecs.Component;
using Game.Service;
using UnityEngine;
using UnityEngine.Pool;
using View.Behaviours;
using Core.Utility;
using Core.Unity;

namespace View
{
	/// <summary>
	/// 얘가 책임지고 게임오브젝트를 생성해서 ECS 월드에 있는 엔티티와 결합 시킴
	/// 이제는 사용하지 않는 컨트롤러
	/// 스폰에 대한 개념이 모두 바뀌었기 때문에
	/// </summary>
	public class SpawnerController : MonoBehaviour
	{
		private readonly List<(Type componentType, Type behaviourType)> componentPairList = new List<(Type componentType, Type behaviourType)>();

		// 추후 init 호출 시점 추가되면 Start 내용 init으로 옮기기
		public void Init()
		{
		}
		
		public void Start()
		{
			// 각 컴포넌트에 대응되는 behaviour 페어 시킴
			foreach (var type in TypeUtility.GetTypesWithInterface(typeof(IComponent)))
			{
				Type binderType = typeof(IComponentBinder<>);
				Type binderImplementType = binderType.MakeGenericType(type);

				var result = TypeUtility.GetTypesWithInterface(binderImplementType);
				
				foreach (var resultType in result)
				{
					componentPairList.Add((type, resultType));
				}
			}

			Spawner.OnSpawnEvent += OnSpawn;
		}

		public void OnDestroy()
		{
			Spawner.OnSpawnEvent -= OnSpawn;
		}

		public void OnSpawn(Entity entity)
		{
			// TODO:오브젝트풀 추가되면 빈 게임오브젝트 가져오는게 좋을듯
			var go = new GameObject();

			foreach (var pair in componentPairList)
			{
				if (entity.Has(pair.componentType))
				{
					if (go.AddComponent(pair.behaviourType) is EcsSuperBehaviour ecsSuperBehaviour)
					{
						ecsSuperBehaviour.SetupEntity(entity);
					}
				}
			}
		}
	}
}
