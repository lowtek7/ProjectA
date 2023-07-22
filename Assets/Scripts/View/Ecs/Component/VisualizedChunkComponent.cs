using Core.Unity;

namespace View.Ecs.Component
{
	public struct VisualizedChunkComponent : IComponent
	{
		public IComponent Clone()
		{
			return new VisualizedChunkComponent();
		}
	}
}
