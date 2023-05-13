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
	[UnityService(typeof(IStageRenderService))]
	public class UnityStagePartitionService : MonoBehaviour, IStagePartitionService
	{
		private readonly Dictionary<Guid, KdTree<float, HashSet<int>>> _treeMap = new Dictionary<Guid, KdTree<float, HashSet<int>>>();

		private Query<TransformComponent, StageSpecComponent> _query;

		/// <summary>
		/// KD-Tree에서 사용 할 차원
		/// </summary>
		private int Dimensions => 3;

		public void Init(World world)
		{
			_query = new Query<TransformComponent, StageSpecComponent>(world);
			_treeMap.Clear();
		}

		void IStagePartitionService.Fetch()
		{
			// 메모리 할당을 최대한 피하기 위해 buffer 할당
			var arrayBuffer = new float[] { 0, 0, 0 };

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

				PosToArrayBuffer(pos, ref arrayBuffer);

				{
					if (tree.TryFindValueAt(arrayBuffer, out var idSet))
					{
						idSet.Add(id);
					}
					else
					{
						idSet = new HashSet<int> { id };
						tree.Add(arrayBuffer.ToArray(), idSet);
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
						PosToArrayBuffer(point, ref arrayBuffer);

						if (tree.TryFindValueAt(arrayBuffer, out var idSet))
						{
							idSet.Add(id);
						}
						else
						{
							idSet = new HashSet<int> { id };
							tree.Add(arrayBuffer.ToArray(), idSet);
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
	}
}
