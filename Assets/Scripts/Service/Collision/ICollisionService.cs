using BlitzEcs;
using UnityEngine;

namespace Service.Collision
{
	public interface ICollisionService : IGameService
	{
		bool IsCollision(Entity moverEntity, Vector3 moveDist);
	}
}
