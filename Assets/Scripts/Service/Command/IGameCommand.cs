using BlitzEcs;

namespace Service.Command
{
	public interface IGameCommand
	{
		void Execute(Entity entity);
	}
}
