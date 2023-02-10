namespace Library.JSPool
{
	/// <summary>
	/// 해당 이벤트를 상속받는 객체는 풀로부터 다양한 이벤트를 콜백 받을 수 있다.
	/// </summary>
	public interface IPoolEvent
	{
		/// <summary>
		/// 풀에서부터 스폰되었을때 호출되는 메서드
		/// </summary>
		void OnSpawned();
		/// <summary>
		/// 풀로 돌아갈 때 호출되는 메서드
		/// </summary>
		void OnDespawned();
	}
}
