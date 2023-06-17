using BlitzEcs;

namespace Core.Unity
{
	public interface ISystem
	{
		/// <summary>
		/// 초기화 함수
		/// </summary>
		void Init(World world);

		/// <summary>
		/// 업데이트 함수
		/// </summary>
		/// <param name="deltaTime"></param>
		void Update(float deltaTime);

		/// <summary>
		/// 모든 업데이트가 호출 된 후에 호출되는 업데이트 함수
		/// </summary>
		/// <param name="deltaTime"></param>
		void LateUpdate(float deltaTime);
	}
}
