using System;

namespace Game.World
{
	/// <summary>
	/// 게임내에서 사용할 각종 상수들을 포함한 클래스
	/// </summary>
	public static class Constants
	{
		public static Guid UnknownStageGuid => Guid.Empty;

		/// <summary>
		/// 청크의 실제 크기 (16 x 16)
		/// </summary>
		public static byte ChunkUnitSize => 1 << 4;

		/// <summary>
		/// 맵 내의 전체 청크 Extents (1024 x 1024)
		/// </summary>
		public static int TotalChunkExtents => 1 << 10;

		/// <summary>
		/// 맵 내의 전체 청크 개수 (2048 x 2048)
		/// </summary>
		public static int TotalChunkSize => TotalChunkExtents << 1;

		/// <summary>
		/// 플레이어 기준 x,y 합산 거리가 16 * 8 이하인 청크 내부 셀만 실제 엔티티로 업데이트할 것임
		/// </summary>
		public static short UpdatableChunkExtents => 1 << 3;
	}
}
