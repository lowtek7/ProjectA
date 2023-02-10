using System.Collections;
using Game.World;
using Library.JSPool;
using UnityEngine;

namespace Game
{
	/// <summary>
	/// 현재 테스트를 위해서 임시적으로 작성한 코드들
	/// </summary>
	public class GameLoader : MonoBehaviour
	{
		[SerializeField]
		private AssetFactory assetFactory;

		[SerializeField]
		private Camera gameCamera;
		
		[SerializeField]
		private PoolManager poolManager;

		private bool canPlay = false;

		private GameManager _gameManager;

		public GameManager GameManager => _gameManager;

		public PoolManager PoolManager => poolManager;

		public Camera Camera => gameCamera;

		public AssetFactory AssetFactory => assetFactory;

		// Component
		// ViewComponent
		// Guid source guid (prefab마다 guid를 보관하고 있을거)
		// guid로 pool에서 gameobject 검색해서 가져와서 사용 할거임.
		// 관측 하고 있는 서비스 같은게 있어야 함
		// pool은 명령 받아서 그냥 실행하는 라이브러리
		// system이 하나 있어야 할듯? view에서만 상주할 시스템 
		// 시스템이 view component 검색(query)해서 정보 얻어오면 pool에서 끌어와서 생성하자.
		
		/// <summary>
		/// 여기서 게임 환경을 로드하게 된다
		/// </summary>
		public void Start()
		{
			canPlay = false;
			// 필요한 에셋들을 로드시키기
			StartCoroutine(LoadAll());
			// manager self register  manager가 모든 게임오브젝트를 update 돌리는게 빠름
		}

		private void Update()
		{
			if (canPlay)
			{
				_gameManager.Update();
			}
		}

		private IEnumerator LoadAll()
		{
			assetFactory.Init(poolManager);
			yield return assetFactory.LoadAll();
			
			// 월드 셋팅
			_gameManager = new GameManager();
			_gameManager.Init(this);
			
			// 이제 플레이 가능한 상태
			canPlay = true;
		}
	}
}
