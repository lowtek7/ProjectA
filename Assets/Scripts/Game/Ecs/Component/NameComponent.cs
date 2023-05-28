using System;
using Core.Unity;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Game.Ecs.Component
{
	[Serializable]
	public struct NameComponent : IComponent
	{
		[SerializeField]
		private string name;

		public string Name => name;
		
		public IComponent Clone()
		{
			return new NameComponent
			{
				name = name
			};
		}
	}
}
