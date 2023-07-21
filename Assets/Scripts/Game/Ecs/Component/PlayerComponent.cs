using System;
using Core.Unity;

namespace Game.Ecs.Component
{
	public enum PlayerType
	{
		Local = 0,
		Remote
	}

	/// <summary>
	/// 플레이어 컴포넌트
	/// </summary>
	[Serializable]
	public struct PlayerComponent : IComponent
	{
		private PlayerType playerType;

		/// <summary>
		/// 플레이어 타입이 로컬과 리모트로 나뉘어 있는데
		/// 리모트는 서버측에서 관리하는 것을 뜻한다.
		/// 추후에 Authority 개념으로 바꿀 예정
		/// </summary>
		public PlayerType PlayerType
		{
			get => playerType;
			set => playerType = value;
		}

		public IComponent Clone()
		{
			return new PlayerComponent
			{
				playerType = playerType
			};
		}
	}
}
