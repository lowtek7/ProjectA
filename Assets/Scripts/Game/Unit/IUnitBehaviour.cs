using BlitzEcs;

namespace Game.Unit
{
	public interface IUnitBehaviour
	{
		/// <summary>
		/// 새로운 엔티티와 연결을 하는 작업
		/// </summary>
		/// <param name="entity"></param>
		void Connect(Entity entity);

		/// <summary>
		/// 현재 연결된 엔티티와 연결을 끊는 작업
		/// </summary>
		void Disconnect();
	}
}
