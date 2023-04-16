using System;
using System.Collections.Generic;
using BlitzEcs;
using Service.Command;
using UnityEngine;
using UnityService.Command.Script;

namespace UnityService.Command
{
	[Serializable]
	struct GameCommandData
	{
		[SerializeField]
		private int typeId;
		
		[SerializeField]
		private GameCommandScriptBase script;

		public GameCommandScriptBase Script => script;

		public int TypeId => typeId;
	}

	[UnityService(typeof(UnityGameCommandService))]
	public class UnityGameCommandService : MonoBehaviour, IGameCommandService
	{
		/// <summary>
		/// 여기는 데이터를 담아두기만 하는 리스트
		/// </summary>
		[SerializeField]
		private List<GameCommandData> dataList = new List<GameCommandData>();

		/// <summary>
		/// 여기서 실제로 데이터를 Map 컨테이너에 담아져서 사용되는 역할을 하게 된다.
		/// </summary>
		private readonly Dictionary<int, IGameCommand> _commandMap = new Dictionary<int, IGameCommand>();

		public void Init(World world)
		{
			_commandMap.Clear();

			foreach (var data in dataList)
			{
				if (!_commandMap.TryAdd(data.TypeId, data.Script))
				{
					Debug.LogError($"UnityGameCommandService Init Error. _commandMap Try Add Failed. [{data.TypeId}, {data.Script.name}]");
				}
			}
		}

		public bool TryGet(int typeId, out IGameCommand gameCommand)
		{
			return _commandMap.TryGetValue(typeId, out gameCommand);
		}
	}
}
