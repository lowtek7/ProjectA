﻿using BlitzEcs;
using Game.Ecs.Component;
using Service.Input;
using UnityEngine;

namespace UnityService.Input
{
	[UnityService(typeof(IGameInputService))]
	public class KeyboardAndMouseGameInputService : MonoBehaviour, IGameInputService
	{
		private World selfWorld;

		private Query<InputComponent> inputQuery;

		private bool activated = false;

		private Entity inputEntity;

		public void Init(World world)
		{
			selfWorld = world;
			inputQuery = new Query<InputComponent>(world);

			// 인풋 관리는 한 곳에서만 진행할 것이므로,
			inputEntity = world.Spawn();
			inputEntity.Add(new InputComponent{ MoveDirection = Vector3.zero });
		}

		void IGameInputService.Fetch()
		{
			int xMove = 0;
			int yMove = 0;

			if (UnityEngine.Input.GetKey(KeyCode.W))
			{
				yMove += 1;
			}

			if (UnityEngine.Input.GetKey(KeyCode.S))
			{
				yMove -= 1;
			}

			if (UnityEngine.Input.GetKey(KeyCode.D))
			{
				xMove += 1;
			}

			if (UnityEngine.Input.GetKey(KeyCode.A))
			{
				xMove -= 1;
			}

			var moveDirection = new Vector3(xMove, yMove, 0).normalized;

			inputQuery.ForEach(((ref InputComponent moveInputComponent) =>
			{
				moveInputComponent.MoveDirection = moveDirection;
			}));
		}
	}
}