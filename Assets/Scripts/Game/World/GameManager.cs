using Core.Unity;
using Library.JSPool;
using UnityEngine;

namespace Game.World
{
	/// <summary>
	/// 임시용 게임 월드
	/// </summary>
	public class GameManager
	{
		private PoolManager poolManager;
		
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
		public void Init(AssetFactory assetFactory, Camera camera, PoolManager poolManager)
		{
			this.poolManager = poolManager;
			poolManager.Init();
			
			// ecs 월드를 생성하자.
			world = new BlitzEcs.World();

			// 1. Service 레이어 구축 (동준님)
			// 유사하게 해야할듯. ECS 
			// 스포너든 뭐 기타 서비스는 나올 수 있는데 그런거 만들어서 통신 할 수 있게 
			// 뷰 영역이나 아래쪽 레이어에서 
			
			// 2. Spawn Manager or Spawner 필요... (혁인님)
			// Service layer 위치해서 그런걸 호출해서 월드에 편하게 스폰하는게 필요하다
			// Spawner를 구독하는 게임오브젝트가 있어야 해서 그걸 받아서 아래 작업해야 할듯

			// 3. 엔티티와 게임 오브젝트의 연결 작업. (형우님)
			// Spawner를 관찰하는 게임오브젝트가 있어야함.
			// spawner는 순수한 엔티티를 생성해서 셋팅까지 하고 건내주는 할 듯.
			// (관찰하고 있다가) 3번에서 월드에 entity가 생성됬네?
			// 그럼 내가 게임 오브젝트 하나 생성해서 entity 바인딩 할게

			// 4. 오브젝트 풀 구현 (지선, 프로파일러 등등)
			// 프로파일링 기능은 있으면 좋다.
			// 어떤어떤게 풀에 나가있고 풀에 들어가있는지 체크가 되야함 풀 사이즈가 얼마
			// 수제 오브젝트 풀 해도 되고 C# 오브젝트 풀 해도 되고 라이브러리 땅겨와도되고

			// 서비스 같은게 돌아가고있어서...
			// 엔티티가 스폰되면 자기가 알아 게임오브젝트 만들어서 ... 추가작업을 진행해서 알아서 조립된다. Factory 

			// 테스트 엔티티를 가져와서 스폰 시키기
			if (assetFactory.TryGetEntityPreset("TestEntity", out var gameObject))
			{
				if (gameObject.TryGetComponent<ComponentDesigner>(out var componentDesigner))
				{
					//엔티티를 서로 연결하는 방법에 대해 생각이 필요함.
					var playerEntity = componentDesigner.ToEntity(world);
					player = GameObject.Instantiate(gameObject, Vector3.zero, Quaternion.identity);

					// Spawner.Spawn("Player").GetComponent<Transform>().Position = Vector3.zero
					// Spawner.Spawn("Player", SpawnInfo)
					// Spawner.Spawn("Player", Position)
					// spawn 내부에서 try get transform position 에다가 스폰하게 해도 될듯 ?
					
					// 컴포넌트 측에서 게임오브젝트에 의존적이면 안된다.
					// Component에서 GameObject를 들고 있는다던가 그런걸 피해야함.
					// GameObject에서 entity 들고 있고 이런건 ok
					// 게임오브젝트의 update에서 계속 관찰 or 
					// 스타일의 차이지 컴포넌트를 런타임중에 삽입 / 삭제를 할지는 
					// 커스텀 update 돌리기 모든 gameobject 
					//

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
