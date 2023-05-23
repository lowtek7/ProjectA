using BlitzEcs;

namespace Service.Field
{
	public interface IFieldRenderService : IGameService
	{
		bool IsRendering(Entity entity);
		void Fetch();
	}
}
