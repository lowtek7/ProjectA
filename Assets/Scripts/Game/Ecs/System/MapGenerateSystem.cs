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

						var capacity = ChunkConstants.MaxBlockCountInChunk;

						// var childBounds = new List<BoundsComponent.ChildBoundsInfo>(capacity);
						var blockIdMap = new ushort[capacity];
						var isSolidMap = new bool[capacity];

						for (int y = 0; y < ChunkConstants.ChunkAxisCount; y++)
						{
							var yWeight = y << ChunkConstants.ChunkAxisExponent;

							for (int x = 0; x < ChunkConstants.ChunkAxisCount; x++)
							{
								var xWeight = x << (ChunkConstants.ChunkAxisExponent << 1);

								for (int z = 0; z < ChunkConstants.ChunkAxisCount; z++)
								{
									var index = xWeight | yWeight | z;

									if (chunkType == 1 && y < ChunkConstants.ChunkAxisCount - 1)
									{
										blockIdMap[index] = 2;
									}
									else
									{
										blockIdMap[index] = chunkType;
									}

									isSolidMap[index] = blockIdMap[index] != ChunkConstants.InvalidBlockId;

									if (isSolidMap[index])
									{
										// childBounds.Add(new BoundsComponent.ChildBoundsInfo
										// {
										// 	centerOffset = new Vector3(x, y, z) + Vector3.one * 0.5f,
										// 	size = Vector3.one
										// });
									}
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
							Position = ChunkUtility.ConvertIdToPos(coordId) * ChunkConstants.ChunkAxisCount
						});

						chunkEntity.Add(new StageSpecComponent
						{
							StageGuid = stagePropertyComponent.StageGuid
						});

						// chunkEntity.Add(new BoundsComponent
						// {
						// 	ChildBounds = childBounds,
						// 	Capacity = childBounds.Capacity,
						// });
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
