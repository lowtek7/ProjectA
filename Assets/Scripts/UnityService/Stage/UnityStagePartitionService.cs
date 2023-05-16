using System;
using System.Collections.Generic;
using System.Linq;
using BlitzEcs;
using Core.Utility;
using Game.Ecs.Component;
using Service.Stage;
using UnityEngine;
using KdTree;
using KdTree.Math;

namespace UnityService.Stage
{
	[UnityService(typeof(IStagePartitionService))]
	public class UnityStagePartitionService : MonoBehaviour, IStagePartitionService
	{
		private readonly Dictionary<Guid, KdTree<float, HashSet<int>>> _treeMap = new Dictionary<Guid, KdTree<float, HashSet<int>>>();

		private World _world;
		
		private Query<TransformComponent, StageSpecComponent> _query;

		private float[] _arrayBuffer;

		/// <summary>
		/// KD-Tree에서 사용 할 차원
		/// </summary>
		private int Dimensions => 3;

		public void Init(World world)
		{
			_world = world;
			_query = new Query<TransformComponent, StageSpecComponent>(world);
			_arrayBuffer = new float[] { 0, 0, 0 };

			_treeMap.Clear();
		}

		void IStagePartitionService.Fetch()
		{
			foreach (var tree in _treeMap.Values)
			{
				tree.Clear();
			}

			_query.ForEach((Entity entity,
				ref TransformComponent transformComponent,
				ref StageSpecComponent stageSpecComponent) =>
			{
				var id = entity.Id;
				var stageGuid = stageSpecComponent.StageGuid;

				if (!_treeMap.ContainsKey(stageGuid))
				{
					_treeMap.Add(stageGuid,
						new KdTree<float, HashSet<int>>(Dimensions, new FloatMath(), AddDuplicateBehavior.Skip));
				}

				var tree = _treeMap[stageGuid];
				var pos = transformComponent.Position;

				PosToArrayBuffer(pos, ref _arrayBuffer);

				{
					if (tree.TryFindValueAt(_arrayBuffer, out var idSet))
					{
						idSet.Add(id);
					}
					else
					{
						idSet = new HashSet<int> { id };
						tree.Add(_arrayBuffer.ToArray(), idSet);
					}
				}

				if (entity.Has<BoundsComponent>())
				{
					var boundsComponent = entity.Get<BoundsComponent>();
					var bounds = boundsComponent.GetBounds(pos);
					var points = bounds.GetPoints();

					// 총 8개의 점을 tree에 기록 해야한다.
					foreach (var point in points)
					{
						PosToArrayBuffer(point, ref _arrayBuffer);

						if (tree.TryFindValueAt(_arrayBuffer, out var idSet))
						{
							idSet.Add(id);
						}
						else
						{
							idSet = new HashSet<int> { id };
							tree.Add(_arrayBuffer.ToArray(), idSet);
						}
					}
				}
			});
		}

		private float[] PosToArray(Vector3 pos)
		{
			return new[] { pos.x, pos.y, pos.z };
		}

		/// <summary>
		/// Array Buffer를 이용해서 pos를 array로 바꾸는 함수.
		/// 최대한 메모리 할당을 막기 위해서 이 방법을 사용한다.
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="array"></param>
		private void PosToArrayBuffer(Vector3 pos, ref float[] array)
		{
			array[0] = pos.x;
			array[1] = pos.y;
			array[2] = pos.z;
		}

		/// <summary>
		/// 해당 엔티티와 겹치는 엔티티들을 모조리 가져오는 함수.
		/// 미리 할당된 list 객체를 넣어주어야 한다.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="overlappedEntities">미리 할당된 list 객체. 겹친 엔티티들을 내뱉는다</param>
		public void GetBoundsOverlappedEntities(Entity entity, ref List<Entity> overlappedEntities)
		{
			var idSet = new HashSet<Entity>();
			overlappedEntities.Clear();

			if (entity.Has<StageSpecComponent>() &&
				entity.Has<BoundsComponent>() &&
				entity.Has<TransformComponent>())
			{
				var stageGuid = entity.Get<StageSpecComponent>().StageGuid;
				var pos = entity.Get<TransformComponent>().Position;
				var bounds = entity.Get<BoundsComponent>().GetBounds(pos);

				if (_treeMap.TryGetValue(stageGuid, out var tree))
				{
					// 모든 사이즈 중 가장 큰 사이즈의 값을 radius로 사용 해야한다.
					var radius = Mathf.Max(Mathf.Max(Mathf.Max(1, bounds.size.x), bounds.size.y), bounds.size.z);

					// 버퍼에 pos을 복사
					PosToArrayBuffer(pos, ref _arrayBuffer);

					var results = tree.RadialSearch(_arrayBuffer, radius);

					foreach (var node in results)
					{
						// 해당 점들과 비교한다.
						if (bounds.Contains(new Vector3(node.Point[0], node.Point[1], node.Point[2])))
						{
							foreach (var id in node.Value)
							{
								idSet.Add(new Entity(_world, id));
							}
						}
					}
				}
			}

			// 자기자신은 결과에서 제외해주자.
			idSet.Remove(entity);
			overlappedEntities.AddRange(idSet);
		}
	}
}
