using System;
using System.Collections.Generic;
using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;

namespace Game.Ecs.System
{
	public class LifeSpanSystem : ISystem
	{
		private BlitzEcs.World _world;
		
		private Query<LifeSpanComponent> _query;

		private readonly List<int> _destroyedEntities = new List<int>(200);

		public Order Order => Order.Lowest;

		public void Init(BlitzEcs.World world)
		{
			_world = world;
			_query = new Query<LifeSpanComponent>(world);

			_destroyedEntities.Clear();
		}

		public void Update(float deltaTime)
		{
			foreach (var entity in _query)
			{
				ref var lifeSpanComponent = ref entity.Get<LifeSpanComponent>();

				switch (lifeSpanComponent.LifeType)
				{
					case LifeType.Infinite:
						break;
					case LifeType.Time:
					{
						lifeSpanComponent.DurationTime -= deltaTime;

						if (lifeSpanComponent.DurationTime <= 0)
						{
							_destroyedEntities.Add(entity);
						}
						break;
					}
					case LifeType.Count:
					{
						--lifeSpanComponent.DurationCount;

						if (lifeSpanComponent.DurationCount <= 0)
						{
							_destroyedEntities.Add(entity);
						}
						break;
					}
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
			foreach (var entity in _destroyedEntities)
			{
				_world.Despawn(entity);
			}
			
			_destroyedEntities.Clear();
		}
	}
}
