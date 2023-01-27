using System.Collections;
using Game.World;
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
		
		private bool canPlay = false;

		private GameWorld gameWorld;
		
		/// <summary>
		/// 여기서 게임 환경을 로드하게 된다
		/// </summary>
		public void Start()
		{
			canPlay = false;
			// 필요한 에셋들을 로드시키기
			StartCoroutine(LoadAll());
		}

		private void Update()
		{
			if (canPlay)
			{
				gameWorld.Update();
			}
		}

		private IEnumerator LoadAll()
		{
			assetFactory.LoadAllSprite();
			
			// 로딩 중이면 기달려주자
			while (assetFactory.IsLoading)
			{
				yield return null;
			}
			
			assetFactory.LoadAllArchetype();

			// 로딩 중이면 기달려주자
			while (assetFactory.IsLoading)
			{
				yield return null;
			}
			
			// 월드 셋팅
			gameWorld = new GameWorld();
			gameWorld.Init(assetFactory, camera);
			
			// 이제 플레이 가능한 상태
			canPlay = true;
		}
	}
}
