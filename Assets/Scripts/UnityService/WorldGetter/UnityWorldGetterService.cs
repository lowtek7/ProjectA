using BlitzEcs;
using Service.World;
using UnityEngine;

namespace UnityService.WorldGetter
{
	[UnityService(typeof(IWorldGetterService))]
	public class UnityWorldGetterService : MonoBehaviour, IWorldGetterService
	{
		private World innerWorld;

		public World World => innerWorld;

		public void Init(World world)
		{
			innerWorld = world;
		}
	}
}
