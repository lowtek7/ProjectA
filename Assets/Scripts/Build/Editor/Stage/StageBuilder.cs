using System;
using System.Collections.Generic;
using System.Linq;
using Build.Stage;
using Core.Unity;
using Core.Virtual;
using Game.Ecs.Component;
using Game.Unit;
using UnityEngine;

namespace Build.Editor.Stage
{
	public static class StageBuilder
	{
		/// <summary>
		/// 스테이지를 Json 데이터로 빌드하는 함수.
		/// </summary>
		/// <param name="stageSetting"></param>
		/// <param name="virtualWorld"></param>
		/// <returns></returns>
		public static void Build(StageSetting stageSetting, VirtualWorld virtualWorld)
		{
			var stageGuid = stageSetting.StageGuid;
			var componentBuffer = new List<IComponent>();
			var unitTemplates = stageSetting.GetComponentsInChildren<UnitTemplate>().ToArray();

			foreach (var unitTemplate in unitTemplates)
			{
				var gameObject = unitTemplate.gameObject;
				var sourceGuid = unitTemplate.SourceGuid;
				// InstanceGuid는 각 엔티티별로 고유한 ID이다.
				var instanceGuid = Guid.NewGuid();

				componentBuffer.Add(new UnitComponent { SourceGuid = sourceGuid });
				componentBuffer.Add(new UnitInstanceComponent { InstanceGuid = instanceGuid });
				componentBuffer.Add(new TransformComponent { Position = gameObject.transform.position });
				componentBuffer.Add(new StageSpecComponent { StageGuid = stageGuid });

				// 컴포넌트 스펙 비헤이비어가 있다면 가져와서 사용해준다.
				if (gameObject.TryGetComponent<ComponentSpecBehaviour>(out var componentSpecBehaviour))
				{
					componentBuffer.AddRange(componentSpecBehaviour.Components);
				}

				// virtual entity 생성해서 component buffer 건내주기
				var virtualEntity = new VirtualEntity(componentBuffer);
				// virtual entity 생성자 내부에서 copy가 발생하기 때문에 component buffer를 청소하면 된다.
				componentBuffer.Clear();
				virtualWorld.AddEntity(virtualEntity);
			}
		}
	}
}
