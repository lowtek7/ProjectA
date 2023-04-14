using System;
using System.Collections.Generic;
using UnityEngine;

namespace Library.JSAnim2D
{
	public enum AnimationLoopType
	{
		Repeat = 0,
		/// <summary>
		/// 애니메이션이 총 3개 있다면
		/// 1 -> 2 -> 3 -> 2 -> 1 -> 2 -> 3 같은 방식으로 루프함
		/// </summary>
		YoYo
	}
	
	[Serializable]
	public struct SpriteInfo
	{
		[SerializeField]
		private Sprite sprite;

		[SerializeField]
		private int frameDuration;

		// 추후에 특정 애니메이션이 이벤트 발동하기 위해 사용 할 키
		//private string eventKey;

		public int FrameDuration
		{
			get => frameDuration;
			set => frameDuration = value;
		}

		public Sprite Sprite
		{
			get => sprite;
			set => sprite = value;
		}
	}
	
	[Serializable]
	public struct AnimationInfo
	{
		[SerializeField]
		private string animationName;
		
		[SerializeField]
		private SpriteInfo[] sprites;

		[SerializeField]
		private AnimationLoopType loopType;

		public SpriteInfo[] Sprites => sprites;

		public AnimationLoopType LoopType => loopType;

		public string AnimationName => animationName;

		public static AnimationInfo Create(string name, int spriteCount)
		{
			return new AnimationInfo { animationName = name, sprites = new SpriteInfo[spriteCount] };
		}
	}
	
	[CreateAssetMenu(menuName = "JSAnim2D/Sprite Animation Data", fileName = "anim_data.asset")]
	public class JSSpriteAnimationData : ScriptableObject
	{
		[SerializeField]
		private List<AnimationInfo> animations = new ();

		[SerializeField]
		private float fps = 60.0f;

		public float FPS => fps;

		public List<AnimationInfo> Animations => animations;
	}
}
