using System;
using System.Collections.Generic;
using Core.Unity;
using UnityEngine;

namespace Game.Ecs.Component
{
	[Serializable]
	public struct MultiBoundsComponent : IComponent, ISerializationCallbackReceiver
	{
		[Serializable]
		public struct ChildBoundsInfo
		{
			public Vector3 centerOffset;
			public Vector3 size;
		}

		[SerializeField]
		private int capacity;

		public int Capacity
		{
			get => capacity;
			set
			{
				capacity = value;

				// 할당된 Capacity가 더 작은 경우에만 갱신해줌
				if (childBounds.Capacity < capacity)
				{
					childBounds.Capacity = capacity;
				}
			}
		}

		[SerializeField]
		private List<ChildBoundsInfo> childBounds;

		public List<ChildBoundsInfo> ChildBounds
		{
			get => childBounds;
			set => childBounds = value;
		}

		public bool TryGetBoundsAt(int childIndex, Vector3 position, out Bounds bounds)
		{
			if (childIndex < childBounds.Count)
			{
				var childInfo = childBounds[childIndex];

				bounds = new Bounds(position + childInfo.centerOffset, childInfo.size);

				return true;
			}

			bounds = new Bounds();

			return false;
		}

		public IComponent Clone()
		{
			return new MultiBoundsComponent
			{
				capacity = capacity,
				// 복사
				childBounds = new List<ChildBoundsInfo>(childBounds)
			};
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			// 역직렬화된 경우 Capacity를 갱신해줌
			if (childBounds == null)
			{
				childBounds = new List<ChildBoundsInfo>(capacity);
			}
			else
			{
				// ChildBounds의 Capacity와 동기화
				Capacity = capacity;
			}
		}
	}
}
