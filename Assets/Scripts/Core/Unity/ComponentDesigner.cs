using System;
using System.Collections.Generic;
using BlitzEcs;
using UnityEngine;

namespace Core.Unity
{
	/// <summary>
	/// 엔티티가 가지고 있는 컴포넌트 정보를 셋팅하기 위한 Designer class
	/// </summary>
	public class ComponentDesigner : MonoBehaviour
	{
		[SerializeReference, SubclassSelector]
		private List<IComponent> components = new List<IComponent>();

		/// <summary>
		/// 해당 컴포넌트 디자이너에 의해서 엔티티를 생성하는 함수
		/// </summary>
		/// <param name="world">엔티티를 생성할 월드</param>
		/// <returns></returns>
		public Entity ToEntity(World world)
		{
			var entity = world.Spawn();

			foreach (var component in components)
			{
				var componentType = component.GetType();

				if (world.TryGetIComponentPool(componentType, out var pool))
				{
					// 현재는 내부에서 박싱 & 언박싱이 일어나고 있기 때문에 이부분에 관해서 최적화가 필요하다.
					// 아마 ToEntity를 이용해서 미리 Entity들을 충분히 Pooling 시켜둔 후 재사용하는 테크닉이 필요 할 것 같아 보인다.
					pool.Add(entity.Id, component.Clone());
				}
			}
			
			return entity;
		}
	}

	/// <summary>
	/// 컴포넌트 디자이너에게 정보를 주기위해서 상속받아야하는 인터페이스
	/// </summary>
	public interface IComponent
	{
		IComponent Clone();
	}
}
