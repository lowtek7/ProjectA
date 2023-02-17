using System;
using System.Collections.Generic;
using BlitzEcs;
using UnityEngine;

namespace Core.Virtual
{
	/// <summary>
	/// Virtual Entity들이 담겨진 World
	/// </summary>
	[Serializable]
	public class VirtualWorld
	{
		[SerializeField]
		private List<VirtualEntity> entities = new List<VirtualEntity>();

		public IEnumerable<VirtualEntity> Entities => entities;

		public void AddEntity(VirtualEntity entity) => entities.Add(entity);

		public void Realize(World world) => entities.ForEach(x => x.Realize(world));
	}
}
