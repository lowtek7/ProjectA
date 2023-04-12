using System;
using System.Collections.Generic;
using UnityEngine;

namespace Library.JSAnim2D
{
	[Serializable]
	public struct SpriteInfo
	{
		[SerializeField]
		private Sprite sprite;

		[SerializeField]
		private float frameDuration;

		public float FrameDuration
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

		public SpriteInfo[] Sprites => sprites;

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

		public List<AnimationInfo> Animations => animations;
	}
}
