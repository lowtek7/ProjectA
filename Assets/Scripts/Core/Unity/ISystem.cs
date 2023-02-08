using BlitzEcs;

namespace Core.Unity
{
	public interface ISystem
	{
		/// <summary>
		/// 시스템의 업데이트 순서 (Order가 낮은 애들이 먼저 업데이트 된다.)
		/// </summary>
		int Order { get; }

		/// <summary>
		/// 초기화 함수
		/// </summary>
		void Init(World world);

		/// <summary>
		/// 업데이트 함수
		/// </summary>
		/// <param name="deltaTime"></param>
		void Update(float deltaTime);
	}
}
