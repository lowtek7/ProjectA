﻿using Core.Unity;

namespace View.Ecs.Component
{
	/// <summary>
	/// 필드 타일 정보
	/// GameObject화 되기 전에도 이 컴포넌트는 붙어있음
	/// </summary>
	public struct FieldCellComponent : IComponent
	{
		public IComponent Clone()
		{
			return new FieldCellComponent();
		}
	}
}