using System;
using System.Collections.Generic;
using Core.Interface;
using Core.Unity;
using UnityEngine;

namespace Game.Ecs.Component
{
	[Serializable]
	public struct CustomBoundsInfo
	{
		/// <summary>
		/// Bounds가 사용할 Key 값
		/// </summary>
		[SerializeField]
		private string key;

		/// <summary>
		/// bounds가 소속한 채널 id
		/// </summary>
		[SerializeField]
		private int channelId;

		/// <summary>
		/// bounds의 크기
		/// </summary>
		[SerializeField]
		private Vector3 boundsSize;

		/// <summary>
		/// bounds의 offset
		/// </summary>
		[SerializeField]
		private Vector3 offset;

		public Bounds GetBounds(Vector3 pos)
		{
			return new Bounds(pos + offset, boundsSize);
		}
	}
	
	[Serializable]
	public struct CustomBoundsComponent : IComponent, ISerializeEventCallback
	{
		[SerializeField]
		private List<CustomBoundsInfo> boundsInfos;

		private Dictionary<string, CustomBoundsInfo> boundsDict;

		public IComponent Clone()
		{
			boundsDict ??= new Dictionary<string, CustomBoundsInfo>();
			boundsInfos ??= new List<CustomBoundsInfo>();

			return new CustomBoundsComponent
			{
				boundsDict = boundsDict,
				boundsInfos = boundsInfos
			};
		}

		public void OnBeforeSerialize()
		{
			if (boundsDict != null)
			{
				boundsInfos ??= new List<CustomBoundsInfo>();
				boundsInfos.Clear();
				boundsInfos.AddRange(boundsDict.Values);
			}
		}

		public void OnAfterSerialize()
		{
			boundsInfos?.Clear();
		}
	}
}
