using System.Collections.Generic;
using UnityEngine;

namespace Library.JSPool
{
	/// <summary>
	/// 풀링을 관리하는 모노비헤이비어
	/// 기본적으로 addressable asset를 사용한다.
	/// </summary>
	public class PoolManager : MonoBehaviour
	{
		class PoolEntity
		{
			private PoolItem original;

			private PoolItem[] pool;
		}

		[SerializeField]
		private List<PoolItem> poolItems = new List<PoolItem>();

		private readonly Dictionary<string, PoolEntity> poolEntities = new Dictionary<string, PoolEntity>();

		/// <summary>
		/// 풀 초기화 작업
		/// 객체들 미리 풀링해두는 함수
		/// 절대 두번 호출해서는 안된다.
		/// </summary>
		public void Init()
		{
			if (poolEntities.Count > 0)
			{
				Debug.LogError("Error! PoolEntities is already is use!");
				poolEntities.Clear();
			}

			foreach (var poolItem in poolItems)
			{
			}
		}
	}
}
