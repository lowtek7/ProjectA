using Core.Unity;

namespace View.Ecs.Component
{
	/// <summary>
	/// 필드 셀의 GameObject화 요청
	/// </summary>
	public struct FieldCellRequestRealizeComponent : IComponent
	{
		public IComponent Clone()
		{
			return new FieldCellRequestRealizeComponent();
		}
	}
}
