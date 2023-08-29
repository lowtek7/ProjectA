using BlitzEcs;

namespace Service.Rendering
{
	public interface IChunkRenderService : IGameService
	{
		int CoordViewDistance { get; }

		void RemoveVisualizer(Entity entity);

		void AddVisualizer(Entity entity);

		void UpdateVisualizer(Entity entity);

		void EndFetch();

		void StartFetch();
	}
}
