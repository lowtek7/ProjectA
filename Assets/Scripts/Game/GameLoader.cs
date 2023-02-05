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
		private Camera camera;
		
		[SerializeField]
		private PoolManager poolManager;
		
		private bool canPlay = false;

		private GameManager _gameManager;

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
			// 스프라이트 다 불러오기
			assetFactory.LoadAllSprite();
			
			// 로딩 중이면 기달려주자
			while (assetFactory.IsLoading)
			{
				yield return null;
			}
			
			assetFactory.LoadAllEntityPreset();

			// 로딩 중이면 기달려주자
			while (assetFactory.IsLoading)
			{
				yield return null;
			}
			
			// 월드 셋팅
			_gameManager = new GameManager();
			_gameManager.Init(assetFactory, camera, poolManager);
			
			// 이제 플레이 가능한 상태
			canPlay = true;
		}
	}
}
