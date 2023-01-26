using UnityEngine;

namespace Game
{
	public class GameLoader : MonoBehaviour
	{
		[SerializeField]
		private AssetFactory assetFactory;
		
		/// <summary>
		/// 여기서 게임 환경을 로드하게 된다
		/// </summary>
		public void Start()
		{
			// 테스트로 에셋 팩토리에서 리소스 불러오기
			assetFactory.LoadAllArchetype();
		}
	}
}
