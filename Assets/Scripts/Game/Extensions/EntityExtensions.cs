using BlitzEcs;
using Game.Ecs.Component;

namespace Game.Extensions
{
	public static class EntityExtensions
	{
		/// <summary>
		/// 이 엔티티가 로컬이 소유한 엔티티인지 검사
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static bool IsLocalEntity(this Entity entity)
		{
			if (entity.Has<NetworkEntityComponent>())
			{
				return entity.Get<NetworkEntityComponent>().EntityRole == EntityRole.Local;
			}

			return true;
		}

		/// <summary>
		/// 이 엔티티가 네트워크에서 소유하고 있는 엔티티인지 검사
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static bool IsRemoteEntity(this Entity entity)
		{
			if (entity.Has<NetworkEntityComponent>())
			{
				return entity.Get<NetworkEntityComponent>().EntityRole == EntityRole.Remote;
			}

			return false;
		}
	}
}
