using System;
using System.Collections.Generic;
using System.Linq;
using BlitzEcs;
using Core.Unity;
using UnityEngine;

namespace Core.Virtual
{
	/// <summary>
	/// 빌드에 쓰거나 엔티티를 가상화 할 경우 VirtualEntity를 활용하면 된다.
	/// 혹은 월드를 저장 할 때도 VirtualEntity를 사용하게 된다.
	/// 다만 VirtualEntity는 성능이 중요한 작업에서 사용해서는 절대로 안된다.
	/// </summary>
	[Serializable]
	public class VirtualEntity
	{
		/// <summary>
		/// Virtualize에서 활용하는 컴포넌트 버퍼
		/// 목적을 달성했다면 항상 비우는것을 잊지 말자.
		/// </summary>
		private static readonly List<IComponent> ComponentBuffer = new List<IComponent>();

		[SerializeReference]
		private IComponent[] components = Array.Empty<IComponent>();

		/// <summary>
		/// 실체화
		/// (퍼포먼스상으로 매우 좋지 않음. boxing/unboxing이 일어나고 있는 상황.)
		/// </summary>
		public void Realize(World world)
		{
			var entity = world.Spawn();
			foreach (var component in components)
			{
				var pool = world.GetIComponentPool(component.GetType());
				pool.Add(entity.Id, component);
			}
		}

		/// <summary>
		/// 컴포넌트를 기록하기 위해서 비효율적인 순회를 하고 있음.
		/// 최적화를 하려면 Ecs Core 구조를 어느정도 수정할 필요가 있기 때문에 보류중
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static VirtualEntity Virtualize(Entity entity)
		{
			ComponentBuffer.Clear();
			var world = entity.World;
			var componentCount = world.ComponentCount;
			var virtualEntity = new VirtualEntity();

			for (int i = 0; i < componentCount; i++)
			{
				if (world.TryGetIComponentPool(i, out var pool))
				{
					if (pool.Contains(entity.Id))
					{
						// boxing 발생.
						if (pool.GetWithBoxing(entity.Id) is IComponent component)
						{
							ComponentBuffer.Add(component);
						}
					}
				}
			}

			// buffer의 데이터를 복사시키기
			virtualEntity.components = ComponentBuffer.ToArray();
			ComponentBuffer.Clear();
			return virtualEntity;
		}

		public VirtualEntity()
		{
			components = Array.Empty<IComponent>();
		}

		public VirtualEntity(IEnumerable<IComponent> buffer)
		{
			components = buffer.ToArray();
		}
	}
}
