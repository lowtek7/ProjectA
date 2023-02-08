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
			assetFactory.Init();
			yield return assetFactory.LoadAll();
			
			// 월드 셋팅
			_gameManager = new GameManager();
			_gameManager.Init(this);
			
			// 이제 플레이 가능한 상태
			canPlay = true;
		}
	}
}
