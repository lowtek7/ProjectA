namespace Game.World.Stage
{

	public class StageMap
	{
		// TODO : 데이터를 SerializeField로 혹은 AssetLoader 통해서 읽어오도록 수정
		// 추후 랜덤 생성 기능이 들어갔을 때에는 비워두면 랜덤, 넣어주면 테스트용 정적 맵 로드하도록 할 예정
		public string[,] map =
		{
			{ "wall", "wall", "wall", "wall", "wall" },
			{ "wall", "floor", "floor", "floor", "wall" },
			{ "wall", "floor", "floor", "floor", "wall" },
			{ "wall", "floor", "floor", "floor", "wall" },
			{ "wall", "floor", "floor", "floor", "wall" },
			{ "wall", "floor", "floor", "floor", "wall" },
			{ "wall", "floor", "floor", "floor", "wall" },
			{ "wall", "floor", "floor", "floor", "wall" },
			{ "wall", "floor", "floor", "floor", "wall" },
			{ "wall", "wall", "wall", "wall", "wall" },
		};
	}
}
