using BlitzEcs;
using Core.Unity;
using Game.Ecs.Component;
using Game.World.Stage;
using UnityEngine;

namespace Game.Ecs.System
{
	public class MapGenerateSystem : ISystem
	{
		private BlitzEcs.World _world;

		private Query<StagePropertyComponent> _stagePropertyQuery;

		public void Init(BlitzEcs.World world)
		{
			_stagePropertyQuery = new Query<StagePropertyComponent>(world);
			_world = world;
		}

		public void Update(float deltaTime)
		{
			foreach (var entity in _stagePropertyQuery)
			{
				ref var stagePropertyComponent = ref entity.Get<StagePropertyComponent>();

				if (stagePropertyComponent.CanGenerate)
				{

					for (var coordId = 0; coordId < ChunkConstants.TotalChunkCount; coordId++)
					{
						// FIXME : 테스트용 코드
						ushort chunkType;
						var coordY = ChunkUtility.GetCoordY(coordId);

						if (coordY >= 0)
						{
							chunkType = ChunkConstants.InvalidBlockId;
						}
						else if (coordY == -1)
						{
							chunkType = 1;
						}
						else
						{
							chunkType = 2;
						}

						var capacity = ChunkConstants.MaxLocalBlockCount;

						var blockIdMap = new ushort[capacity];
						var isSolidMap = new bool[capacity];

						for (int y = 0; y < ChunkConstants.LocalBlockAxisCount; y++)
						{
							for (int x = 0; x < ChunkConstants.LocalBlockAxisCount; x++)
							{
								for (int z = 0; z < ChunkConstants.LocalBlockAxisCount; z++)
								{
									var localBlockId = ChunkUtility.GetLocalBlockId(x, y, z);

									if (chunkType == 1 && y < ChunkConstants.LocalBlockAxisCount - 1)
									{
										blockIdMap[localBlockId] = 2;
									}
									else
									{
										blockIdMap[localBlockId] = chunkType;
									}

									isSolidMap[localBlockId] = blockIdMap[localBlockId] != ChunkConstants.InvalidBlockId;
								}
							}
						}

						var chunkEntity = _world.Spawn();

						chunkEntity.Add(new ChunkComponent
						{
							coordId = coordId,
							blockIdMap = blockIdMap,
							isSolidMap = isSolidMap,
						});

						chunkEntity.Add(new TransformComponent
						{
							Direction = Vector3.up,
							Position = ChunkUtility.ConvertIdToPos(coordId) * ChunkConstants.LocalBlockAxisCount
						});

						chunkEntity.Add(new StageSpecComponent
						{
							StageGuid = stagePropertyComponent.StageGuid
						});
					}

					stagePropertyComponent.CanGenerate = false;
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
		}
	}
}
