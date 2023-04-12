using UnityEngine;

namespace Library.JSAnim2D
{
	public class JSAnimator : MonoBehaviour
	{
		[SerializeField]
		private SpriteRenderer spriteRenderer;

		[SerializeField]
		private JSSpriteAnimationData animationData;

		private int _currentAnimIndex = InvalidIndex;

		private int _currentSpriteIndex = InvalidIndex;

		private static int InvalidIndex => -1;

		/// <summary>
		/// 내부 정보를 clear 해주는 함수
		/// </summary>
		public void Clear()
		{
			_currentSpriteIndex = InvalidIndex;
			_currentAnimIndex = InvalidIndex;
		}

		public void SetAnimationData(JSSpriteAnimationData data)
		{
			Clear();
			animationData = data;
		}

		public void Play(string animName)
		{
			Clear();

			_currentAnimIndex = animationData.Animations.FindIndex(x => x.AnimationName == animName);

			var currentAnim = animationData.Animations[_currentAnimIndex];
			
			if (_currentAnimIndex != InvalidIndex && currentAnim.Sprites.Length > 0)
			{
				_currentSpriteIndex = 0;
				//spriteRenderer.sprite = currentAnim.Sprites[_currentSpriteIndex];
			}
		}

		/// <summary>
		/// 성능을 위해 업데이트는 직접 호출하여 돌리게 해준다.
		/// </summary>
		/// <param name="deltaTime"></param>
		public void AnimationUpdate(float deltaTime)
		{
			if (!enabled || _currentAnimIndex == InvalidIndex)
			{
				return;
			}
		}
	}
}
