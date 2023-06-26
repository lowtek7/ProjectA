using System;
using Core.Unity;
using UnityEngine;


namespace Game.Ecs.Component
{
	/// <summary>
	/// 마우스 커서 컴포넌트
	/// </summary>
	[Serializable]
	
	public struct CursorComponent : IComponent
	{
		[SerializeField]
		private bool isShowCursor;
		
		public bool IsShowCursor
		{
			get => isShowCursor;
			
			set => isShowCursor = value;
		}
		
		public IComponent Clone()
		{
			return new CursorComponent
			{
				isShowCursor = isShowCursor
			};
		}
	}
}
