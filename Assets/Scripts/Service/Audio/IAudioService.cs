using UnityEngine;

namespace Service.Audio
{
	public interface IAudioService : IGameService
	{
		/// <summary>
		/// 테스트 플레이 함수
		/// </summary>
		void TestPlayOneShotSFX(string sfxName, Vector3 pos);
	}
}
