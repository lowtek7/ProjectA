using BlitzEcs;
using Service.Command;
using UnityEngine;

namespace UnityService.Command.Script
{
	public abstract class GameCommandScriptBase : ScriptableObject, IGameCommand
	{
		public abstract void Execute(Entity entity, float magnitude);
	}
}
