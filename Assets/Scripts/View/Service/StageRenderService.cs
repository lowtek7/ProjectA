using System;
using System.Collections;
using System.Collections.Generic;
using BlitzEcs;
using Game;
using Game.Ecs.Component;
using Game.Unit;
using UnityEngine;

namespace View.Service
{
	/// <summary>
	/// 현재는 싱글톤으로 구현하였지만 차후에는 서비스 레이어에 해당 서비스를 넣도록 수정해야한다
	/// </summary>
	public class StageRenderService : MonoBehaviour
	{
		private static bool instanceFlag = false;
		private static StageRenderService instance;

		/// <summary>
		/// 모노비헤이비어 널 체크는 퍼포먼스적으로 큰 작업이기 때문에 (라이더가 알려줌)
		/// 인스턴스의 널체크를 instanceFlag를 이용해 대신 한다
		/// </summary>
		/// <param name="stageRenderService"></param>
		/// <returns></returns>
		public static bool TryGetInstance(out StageRenderService stageRenderService)
		{
			stageRenderService = instance;
			return instanceFlag;
		}

		private World selfWorld = null;

		private readonly List<UnitTemplate> loadedUnits = new List<UnitTemplate>();

		private void Awake()
		{
			instance = this;
			instanceFlag = true;
		}

		private void OnDestroy()
		{
			instance = null;
			instanceFlag = false;
		}

		public void Init(World world)
		{
			selfWorld = world;
		}

		/// <summary>
		/// 특정한 스테이지로 이동하는 함수
		/// </summary>
		public void StageTransition(int stageId)
		{
			// 여기서 로드 되어있는 스테이지는 언로드하고 새로운 스테이지는 로드
		}

		private IEnumerator StageTransitionProcess()
		{
			// UI적으로 페이드 먹이거나 하는 것도 여기서 해줘야 함. (검은 화면으로 로딩하는 모습을 가리기 위해)
			foreach (var unit in loadedUnits)
			{
				unit.Disconnect();
			}

			yield return null;

			var assetFactory = AssetFactory.Instance;
			var query = new Query<UnitComponent>(selfWorld);
			query.Fetch();

			foreach (var entity in query)
			{
				
			}
		}
	}
}
