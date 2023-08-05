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
		private readonly Dictionary<Guid, KdTree<float, HashSet<PartitionBoundsInfo>>> _treeMap = new();

		private World _world;

		private Query<TransformComponent, StageSpecComponent> _query;

		private float[] _arrayBuffer;

		private Vector3[] pointBuffer = new Vector3[8];

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

			foreach (var entity in _query)
			{
				var stageSpecComponent = entity.Get<StageSpecComponent>();
				var transformComponent = entity.Get<TransformComponent>();

				var stageGuid = stageSpecComponent.StageGuid;

				if (!_treeMap.ContainsKey(stageGuid))
				{
					_treeMap.Add(stageGuid,
						new KdTree<float, HashSet<PartitionBoundsInfo>>(Dimensions, new FloatMath(), AddDuplicateBehavior.Skip));
				}

				var tree = _treeMap[stageGuid];
				var pos = transformComponent.Position;

				if (entity.Has<BoundsComponent>())
				{
					var boundsComponent = entity.Get<BoundsComponent>();

					for (int i = 0; i < boundsComponent.ChildBounds.Count; i++)
					{
						if (!boundsComponent.TryGetBoundsAt(i, pos, out var bounds))
						{
							continue;
						}

						bounds.GetPoints(ref pointBuffer);

						// 총 8개의 점을 tree에 기록 해야한다.
						foreach (var point in pointBuffer)
						{
							PosToArrayBuffer(point, ref _arrayBuffer);

							var boundsInfo = new PartitionBoundsInfo
							{
								entity = entity,
								boundsIndex = i,
							};

							if (tree.TryFindValueAt(_arrayBuffer, out var idSet))
							{
								idSet.Add(boundsInfo);
							}
							else
							{
								idSet = new HashSet<PartitionBoundsInfo> { boundsInfo };
								tree.Add(_arrayBuffer.ToArray(), idSet);
							}
						}
					}
				}
			}
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
		/// <param name="overlappedBoundsInfos">미리 할당된 list 객체. 겹친 엔티티의 바운드 정보를 내뱉는다</param>
		public void GetBoundsOverlappedEntities(Entity entity, ref List<PartitionBoundsInfo> overlappedBoundsInfos)
		{
			var infoSet = new HashSet<PartitionBoundsInfo>();
			overlappedBoundsInfos.Clear();

			if (entity.Has<StageSpecComponent>() &&
				entity.Has<BoundsComponent>() &&
				entity.Has<TransformComponent>())
			{
				var stageGuid = entity.Get<StageSpecComponent>().StageGuid;
				var pos = entity.Get<TransformComponent>().Position;
				var boundsComponent = entity.Get<BoundsComponent>();

				for (int i = 0; i < boundsComponent.ChildBounds.Count; i++)
				{
					if (!boundsComponent.TryGetBoundsAt(i, pos, out var bounds))
					{
						continue;
					}

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
								foreach (var boundsInfo in node.Value)
								{
									// 자기자신은 결과에서 제외
									if (boundsInfo.entity.Id != entity.Id)
									{
										infoSet.Add(boundsInfo);
									}
								}
							}
						}
					}
				}
			}

			overlappedBoundsInfos.AddRange(infoSet);
		}

		// public void GetAllOnRay(Vector3 startPos, Vector3 endPos,
		// 	ref List<Entity> overlappedEntities, bool ignoreStartPos = true)
		// {
		//
		// }
		//
		// public PartitionBoundsInfo GetRaycasted(Vector3 startPos, Vector3 endPos, bool ignoreStartPos = true)
		// {
		// 	return new PartitionBoundsInfo();
		// }
	}
}
