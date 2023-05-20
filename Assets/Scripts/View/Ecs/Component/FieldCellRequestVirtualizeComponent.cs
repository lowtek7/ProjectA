using Core.Unity;

namespace View.Ecs.Component
{
	/// <summary>
	/// 필드 셀의 Virtualize 요청
	/// </summary>
	public struct FieldCellRequestVirtualizeComponent : IComponent
	{
		public IComponent Clone()
		{
			return new FieldCellRequestVirtualizeComponent();
		}
	}
}
