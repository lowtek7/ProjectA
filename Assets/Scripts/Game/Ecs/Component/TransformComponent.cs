using System;
using Core.Unity;
using UnityEngine;

namespace Game.Ecs.Component
{
	[Flags]
	public enum Direction2D
	{
		None = 0,
		Up = 1,
		Left = 2,
		Down = 4,
		Right = 8,
	}

	/// <summary>
	/// 위치와 연관된 데이터를 기록하는 컴포넌트
	/// </summary>
	[Serializable]
	public struct TransformComponent : IComponent
	{
		[SerializeField]
		private Vector3 position;

		public Vector3 Position
		{
			get => position;
			set => position = value;
		}

		[SerializeField]
		private Vector2 direction;

		public Vector2 Direction
		{
			get => direction;
			set => direction = value;
		}

		public Direction2D Direction2D
		{
			get
			{
				var result = Direction2D.None;

				// 별거 아니지만 작은 최적화를 위해서라도 스위치 케이스로 로직을 동작하게 변경
				switch (direction.x)
				{
					case > 0:
						result |= Direction2D.Right;
						break;
					case < 0:
						result |= Direction2D.Left;
						break;
				}

				switch (direction.y)
				{
					case > 0:
						result |= Direction2D.Up;
						break;
					case < 0:
						result |= Direction2D.Down;
						break;
				}

				return result;
			}
			set
			{
				direction = Vector2.zero;

				if ((value & Direction2D.Up) != 0)
				{
					direction.y = 1;
				}
				else if ((value & Direction2D.Down) != 0)
				{
					direction.y = -1;
				}

				if ((value & Direction2D.Left) != 0)
				{
					direction.x = -1;
				}
				else if ((value & Direction2D.Right) != 0)
				{
					direction.x = 1;
				}

				direction.Normalize();
			}
		}

		public string DirectionName
		{
			get
			{
				switch (direction.y)
				{
					case > 0:
						return "up";
					case < 0:
						return "down";
					default:
					{
						switch (direction.x)
						{
							case > 0:
								return "right";
							case < 0:
								return "left";
						}

						break;
					}
				}

				return "down";
			}
		}

		public IComponent Clone()
		{
			return new TransformComponent
			{
				position = position,
				direction = direction
			};
		}
	}
}
