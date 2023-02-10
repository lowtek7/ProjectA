using System;
using Core.Unity;
using Core.Utility;
using UnityEngine;

namespace Game.Ecs.Component
{
	[Serializable]
	public struct UnitComponent : IComponent
	{
		[SerializeField]
		private SGuid sourceGuid;

		public Guid SourceGuid
		{
			get => sourceGuid.Guid;
			set => sourceGuid.Guid = value;
		}

		public IComponent Clone()
		{
			return new UnitComponent { sourceGuid = sourceGuid };
		}
	}
}
