using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
	
	/// <summary>
	/// 풀링할 에셋에는 항상 PoolController 모노비헤이비어를 추가해줘야 한다.
	/// 만약 추가하지 않으면 풀로 반환 될때 이벤트를 캐치 받을 수 없다.
	/// </summary>
	public class PoolController : MonoBehaviour
	{
		/// <summary>
		/// 풀 매니저를 제외하고 다른데서 호출하는 행위 절대 금지.
		/// </summary>
		public void SpawnEvent()
		{
			var components = gameObject.GetComponents<IPoolEvent>();

			foreach (var poolEvent in components)
			{
				poolEvent.OnSpawned();
			}
		}
		
		/// <summary>
		/// 풀 매니저를 제외하고 다른데서 호출하는 행위 절대 금지.
		/// </summary>
		public void DespawnEvent()
		{
			var components = gameObject.GetComponents<IPoolEvent>();

			foreach (var poolEvent in components)
			{
				poolEvent.OnDespawned();
			}
		}
	}
}
