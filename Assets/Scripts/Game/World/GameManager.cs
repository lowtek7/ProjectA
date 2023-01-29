using Core.Unity;
using UnityEngine;

namespace Game.World
{
	/// <summary>
	/// 임시용 게임 월드
	/// </summary>
	public class GameManager
	{
		private GameObject player;
		private GameObject enemy;
		private GameObject camera;

		private Vector3 cameraDist = new Vector3(0, 0, -3);

		private float moveSpeed = 10;

		private BlitzEcs.World world;

		/// <summary>
		/// 원래는 에셋 팩토리에 직접 접근하는 일이 없어야 함
		/// 현재 엔티티 스폰을 위해서 임시적으로 이렇게 구성했음
		/// 추후에 spawner를 따로 만들거나 따로 그러한 작업을 위한 서비스를 구축해야함
		/// 카메라 또한 카메라 엔티티를 따로 만들어서 컨트롤 해야함
		/// </summary>
		/// <param name="assetFactory"></param>
		public void Init(AssetFactory assetFactory, Camera camera)
		{
			// ecs 월드를 생성하자.
			world = new BlitzEcs.World();

			// 테스트 엔티티를 가져와서 스폰 시키기
			if (assetFactory.TryGetEntityPreset("TestEntity", out var gameObject))
			{
				if (gameObject.TryGetComponent<ComponentDesigner>(out var componentDesigner))
				{
					//엔티티를 서로 연결하는 방법에 대해 생각이 필요함.
					var playerEntity = componentDesigner.ToEntity(world);
					player = GameObject.Instantiate(gameObject, Vector3.zero, Quaternion.identity);

					var enemyEntity = componentDesigner.ToEntity(world);
					enemy = GameObject.Instantiate(gameObject, new Vector3(1, 1, 0), Quaternion.identity);
				}

				this.camera = camera.gameObject;
			}
		}

		/// <summary>
		/// 추후에 유니티 시간이 아닌 내부 월드 타임을 따로 두어서 사용 할 것.
		/// </summary>
		public void Update()
		{
			var dt = Time.deltaTime;

			if (Input.GetKeyDown(KeyCode.W))
			{
				player.transform.position += (new Vector3(0, moveSpeed, 0) * dt);
			}

			if (Input.GetKeyDown(KeyCode.S))
			{
				player.transform.position -= (new Vector3(0, moveSpeed, 0) * dt);
			}

			if (Input.GetKeyDown(KeyCode.A))
			{
				player.transform.position -= (new Vector3(moveSpeed, 0, 0) * dt);
			}

			if (Input.GetKeyDown(KeyCode.D))
			{
				player.transform.position += (new Vector3(moveSpeed, 0, 0) * dt);
			}

			camera.transform.position = player.transform.position + cameraDist;
		}
	}
}
