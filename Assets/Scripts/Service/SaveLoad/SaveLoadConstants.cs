namespace Service.SaveLoad
{
	public static class SaveLoadConstants
	{
		/// <summary>
		/// World 데이터의 경로
		/// 현재 BuildConstants에도 존재하므로 중복 데이터...
		/// 어딘가에 하나만 두는게 좋을듯
		/// </summary>
		public static string WorldDataPath => "Assets/StaticData/World.json";
	}
}
