using System;
using UnityEngine;

namespace Library.JSAnim2D
{
	public class JSAnimator : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer spriteRenderer;

		[SerializeField] private JSSpriteAnimationData animationData;

		/// <summary>
		/// 현재 값만 미리 준비해둔 상태. 미구현
		/// </summary>
		[SerializeField] private int fallbackAnimationIndex = InvalidIndex;

		private int _currentAnimIndex = InvalidIndex;

		private int _currentSpriteIndex = InvalidIndex;

		/// <summary>
		/// 해당 변수가 false이면 yoyo 에서 줄어드는 상태.
		/// </summary>
		private bool _yoyoIncreaseMode = true;

		private float _timePassed = 0f;

		private float _currentFrameRate = DefaultFrameRate;

		private float _timePerFrame = 0f;

		private static int InvalidIndex => -1;

		private static float DefaultFrameRate => 60.0f;

		/// <summary>
		/// 내부 애니메이션을 리셋하는 함수
		/// </summary>
		public void ResetAnimation()
		{
			_currentSpriteIndex = InvalidIndex;
			_currentAnimIndex = InvalidIndex;
			_yoyoIncreaseMode = true;
			_timePassed = 0;
			_currentFrameRate = DefaultFrameRate;
			_timePerFrame = 1 / _currentFrameRate;
		}

		public void SetAnimationData(JSSpriteAnimationData data)
		{
			ResetAnimation();
			animationData = data;
		}

		public void Play(string animationName)
		{
			var targetAnimIndex = animationData.Animations.FindIndex(x => x.AnimationName == animationName);

			if (targetAnimIndex == _currentAnimIndex)
			{
				return;
			}

			ResetAnimation();

			_currentAnimIndex = targetAnimIndex;

			if (_currentAnimIndex != InvalidIndex)
			{
				var currentAnim = animationData.Animations[_currentAnimIndex];

				_currentFrameRate = animationData.FPS;
				_timePerFrame = 1 / _currentFrameRate;
				_currentSpriteIndex = 0;
				SetSprite(currentAnim.Sprites[_currentSpriteIndex].Sprite);
			}
		}

		/// <summary>
		/// 성능을 위해 업데이트는 직접 호출하여 돌리게 해준다.
		/// </summary>
		/// <param name="deltaTime"></param>
		public void AnimationUpdate(float deltaTime)
		{
			if (!enabled || animationData == null || _currentAnimIndex == InvalidIndex ||
			    !ContainsAnimation(_currentAnimIndex))
			{
				if (enabled)
				{
					if (animationData == null)
					{
						// error
						Debug.LogError($"Error! Animation Data is Null. {gameObject.name}");
					}

					ResetAnimation();
				}

				return;
			}

			var currentAnim = animationData.Animations[_currentAnimIndex];
			var currentSprite = currentAnim.Sprites[_currentSpriteIndex];

			_timePassed += deltaTime;

			if (Convert.ToSingle(currentSprite.FrameDuration) * _timePerFrame <= _timePassed)
			{
				switch (currentAnim.LoopType)
				{
					case AnimationLoopType.Repeat:
						_currentSpriteIndex++;
						break;
					case AnimationLoopType.YoYo:
						if (_yoyoIncreaseMode)
						{
							_currentSpriteIndex++;
							if (_currentSpriteIndex >= currentAnim.Sprites.Length)
							{
								_currentSpriteIndex--;
								_yoyoIncreaseMode = false;
							}
						}
						else
						{
							_currentSpriteIndex--;
							if (_currentSpriteIndex < 0)
							{
								_currentSpriteIndex = 0;
								_yoyoIncreaseMode = true;
							}
						}

						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				if (_currentSpriteIndex < 0 || _currentSpriteIndex >= currentAnim.Sprites.Length)
				{
					_currentSpriteIndex = 0;
				}

				currentSprite = currentAnim.Sprites[_currentSpriteIndex];
				SetSprite(currentSprite.Sprite);

				_timePassed = 0;
			}
		}

		/// <summary>
		/// 해당 애니메이션 데이터에 포함된 애니메이션 인덱스인지 검사.
		/// 이름을 Valid와 연관된 이름으로 바꾸야 할지도 모름
		/// </summary>
		/// <param name="animationIndex"></param>
		/// <returns></returns>
		public bool ContainsAnimation(int animationIndex)
		{
			if (animationData != null)
			{
				return animationData.Animations.Count > animationIndex;
			}

			return false;
		}

		private void SetSprite(Sprite sprite)
		{
			spriteRenderer.sprite = sprite;
		}
	}
}
