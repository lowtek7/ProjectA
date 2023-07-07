using System;
using BlitzEcs;
using Game.Ecs.Component;
using Service;
using Service.Collision;
using UnityEngine;

namespace UnityService.Collision
{
	[UnityService(typeof(ICollisionService))]
	public class UnityCollisionService : MonoBehaviour, ICollisionService
	{
		private Query<TransformComponent, CapsuleColliderComponent> _capsuleColliderQuery;

		bool ICollisionService.IsCollision(Entity moverEntity, Vector3 moveDist)
		{
			if (!moverEntity.Has<CapsuleColliderComponent>())
			{
				return false;
			}

			bool result = false;

			foreach (var targetEntity in _capsuleColliderQuery)
			{
				// 본인 제외
				if (moverEntity.Id == targetEntity.Id)
				{
					continue;
				}

				// 이동 오브젝트 정보
				var moverCapsuleColliderComponent = moverEntity.Get<CapsuleColliderComponent>();
				var moverTransformComponent = moverEntity.Get<TransformComponent>();

				var moverPos = moverTransformComponent.Position + moveDist;
				var moverCapsuleCenter = moverCapsuleColliderComponent.Center;
				var moverCapsuleDir = moverCapsuleColliderComponent.Direction;
				var moverCapsuleHeight = moverCapsuleColliderComponent.Height;

				var moverCapsuleBase = moverPos + moverCapsuleCenter - moverCapsuleDir * (moverCapsuleHeight * 0.5f);
				var moverCapsuleTip = moverCapsuleBase + moverCapsuleDir * moverCapsuleHeight;

				Vector3 moverLineEndOffset = moverCapsuleDir * moverCapsuleColliderComponent.Radius;
				Vector3 moverBaseCircleCenter = moverCapsuleBase + moverLineEndOffset;
				Vector3 moverTipCircleCenter = moverCapsuleTip - moverLineEndOffset;

				// 충돌 오브젝트 정보
				var targetCapsuleColliderComponent = targetEntity.Get<CapsuleColliderComponent>();
				var targetTransformComponent = targetEntity.Get<TransformComponent>();

				var targetPos = targetTransformComponent.Position;
				var targetCapsuleCenter = targetCapsuleColliderComponent.Center;
				var targetCapsuleDir = targetCapsuleColliderComponent.Direction;
				var targetCapsuleHeight = targetCapsuleColliderComponent.Height;

				var targetCapsuleBase = targetPos + targetCapsuleCenter - targetCapsuleDir * (targetCapsuleHeight * 0.5f);
				var targetCapsuleTip = targetCapsuleBase + targetCapsuleDir * targetCapsuleHeight;

				Vector3 targetLineEndOffset = targetCapsuleDir * targetCapsuleColliderComponent.Radius;
				Vector3 targetBaseCircleCenter = targetCapsuleBase + targetLineEndOffset;
				Vector3 targetTipCircleCenter = targetCapsuleTip - targetLineEndOffset;

				// 캡슐끼리 충돌 계산
				Vector3 v0 = targetBaseCircleCenter - moverBaseCircleCenter;
				Vector3 v1 = targetTipCircleCenter - moverBaseCircleCenter;
				Vector3 v2 = targetBaseCircleCenter - moverTipCircleCenter;
				Vector3 v3 = targetBaseCircleCenter - moverTipCircleCenter;

				float d0 = Vector3.Dot(v0, v0);
				float d1 = Vector3.Dot(v1, v1);
				float d2 = Vector3.Dot(v2, v2);
				float d3 = Vector3.Dot(v3, v3);

				Vector3 moverBestCircleCenter;

				if (d2 < d0 || d2 < d1 || d3 < d0 || d3 < d1)
				{
					moverBestCircleCenter = moverTipCircleCenter;
				}
				else
				{
					moverBestCircleCenter = moverBaseCircleCenter;
				}

				Vector3 ClosestPointOnLineSegment(Vector3 a, Vector3 b, Vector3 point)
				{
					Vector3 ab = b - a;
					float t = Vector3.Dot(point - a, ab) / Vector3.Dot(ab, ab);
					return a + Math.Min(Math.Max(t, 0), 1) * ab;
				}

				Vector3 targetBestCircleCenter = ClosestPointOnLineSegment(targetBaseCircleCenter,
					targetTipCircleCenter, moverBestCircleCenter);

				moverBestCircleCenter = ClosestPointOnLineSegment(moverBaseCircleCenter,
					moverTipCircleCenter, targetBestCircleCenter);

				float dist = (moverBestCircleCenter - targetBestCircleCenter).magnitude;
				result = moverCapsuleColliderComponent.Radius + targetCapsuleColliderComponent.Radius - dist > 0;
			}

			return result;
		}

		public void Init(World world)
		{
			_capsuleColliderQuery = new Query<TransformComponent, CapsuleColliderComponent>(world);
		}
	}
}
