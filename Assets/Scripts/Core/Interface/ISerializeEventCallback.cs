namespace Core.Interface
{
	/// <summary>
	/// 해당 인터페이스를 구현하는 컴포넌트는 게임 내부에서 시리얼라이즈가 발생 할 때 콜백 이벤트를 받을 수 있다.
	/// </summary>
	public interface ISerializeEventCallback
	{
		/// <summary>
		/// 시리얼라이즈가 실행되기 직전에 발생하는 이벤트
		/// </summary>
		void OnBeforeSerialize();

		/// <summary>
		/// 시리얼라이즈가 실행 된 후 발생하는 이벤트
		/// </summary>
		void OnAfterSerialize();
	}

	/// <summary>
	/// 해당 인터페이스를 구현한 컴포넌트는 디시리얼라이즈가 발생 한 후 이벤트를 콜백 받을 수 있다.
	/// </summary>
	public interface IDeserializeEventCallback
	{
		/// <summary>
		/// 디시리얼라이즈가 실행 된 후 발생하는 이벤트
		/// </summary>
		void OnAfterDeserialize();
	}
}
