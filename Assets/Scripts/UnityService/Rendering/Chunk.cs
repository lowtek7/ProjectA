using System.Collections.Generic;
using System.Runtime.InteropServices;
using Service;
using Service.Rendering;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UnityService.Rendering
{
	public class Chunk : MonoBehaviour, IChunk
	{
		private MeshFilter _meshFilter;
		private MeshRenderer _meshRenderer;

		private int[] _blockIdMap;

		public GameObject GameObject => gameObject;

		public Vector3Int Coord { get; private set; }

		private bool[] _isSolidMap;

		public bool[] IsSolidMap => _isSolidMap;

		// FIXME : 테스트용
		private string _chunkType;

		private BuildingMeshJob? _currentBuildingMeshJob;
		private JobHandle? _currentBuildingMeshJobHandler;

		private struct BuildingMeshJob : IJob
		{
			public NativeList<Vector3> vertices;
			public NativeList<int> triangles;
			public NativeList<Vector2> uvs;

			public NativeArray<int> blockMap;

			[ReadOnly]
			private NativeArray<bool> _isSolidSelf;
			[ReadOnly]
			private NativeArray<bool> _isSolidLeft;
			[ReadOnly]
			private NativeArray<bool> _isSolidRight;
			[ReadOnly]
			private NativeArray<bool> _isSolidUp;
			[ReadOnly]
			private NativeArray<bool> _isSolidDown;
			[ReadOnly]
			private NativeArray<bool> _isSolidForward;
			[ReadOnly]
			private NativeArray<bool> _isSolidBack;

			private int _triangleVertexWalker;

			public BuildingMeshJob(int[] blockIdMap, bool[] isSolidSelf, bool[] isSolidLeft, bool[] isSolidRight,
				bool[] isSolidUp, bool[] isSolidDown, bool[] isSolidForward, bool[] isSolidBack)
			{
				var lifeType = Allocator.TempJob;

				_isSolidSelf = new NativeArray<bool>(isSolidSelf, lifeType);
				_isSolidLeft = new NativeArray<bool>(isSolidLeft, lifeType);
				_isSolidRight = new NativeArray<bool>(isSolidRight, lifeType);
				_isSolidUp = new NativeArray<bool>(isSolidUp, lifeType);
				_isSolidDown = new NativeArray<bool>(isSolidDown, lifeType);
				_isSolidForward = new NativeArray<bool>(isSolidForward, lifeType);
				_isSolidBack = new NativeArray<bool>(isSolidBack, lifeType);

				// Capacity를 미리 크게 잡아둠
				var normalSideCount =
					VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.BlockSideCount;

				vertices = new NativeList<Vector3>(normalSideCount * 4, lifeType);
				triangles = new NativeList<int>(normalSideCount * 2, lifeType);
				uvs = new NativeList<Vector2>(normalSideCount * 4, lifeType);

				_triangleVertexWalker = 0;

				blockMap = new NativeArray<int>(blockIdMap, lifeType);
			}

			public void Dispose()
			{
				vertices.Dispose();
				triangles.Dispose();
				uvs.Dispose();

				blockMap.Dispose();

				_isSolidSelf.Dispose();
				_isSolidLeft.Dispose();
				_isSolidRight.Dispose();
				_isSolidUp.Dispose();
				_isSolidDown.Dispose();
				_isSolidForward.Dispose();
				_isSolidBack.Dispose();
			}

			public void Execute()
			{
				for (int y = 0; y < VoxelConstants.ChunkAxisCount; y++)
				{
					var yWeight = y << VoxelConstants.ChunkAxisExponent;

					for (int x = 0; x < VoxelConstants.ChunkAxisCount; x++)
					{
						var xWeight = x << (VoxelConstants.ChunkAxisExponent << 1);

						for (int z = 0; z < VoxelConstants.ChunkAxisCount; z++)
						{
							var index = xWeight | yWeight | z;

							if (blockMap[index] >= 0) {
								AddBlock(x, y, z, blockMap[index]);
							}
						}
					}
				}
			}

			private void AddBlock(int x, int y, int z, int blockId)
			{
				var pos = new Vector3(x, y, z);

				for (int s = 0; s < VoxelConstants.BlockSideCount; s++)
				{
					var nearX = x;
					var nearY = y;
					var nearZ = z;
					NativeArray<bool> isSolidData = _isSolidSelf;

					switch (s)
					{
						case 0:
							if (--nearZ < 0)
							{
								nearZ = VoxelConstants.ChunkAxisCount - 1;
								isSolidData = _isSolidBack;
							}

							break;
						case 1:
							if (++nearX >= VoxelConstants.ChunkAxisCount)
							{
								nearX = 0;
								isSolidData = _isSolidRight;
							}

							break;
						case 2:
							if (++nearZ >= VoxelConstants.ChunkAxisCount)
							{
								nearZ = 0;
								isSolidData = _isSolidForward;
							}

							break;
						case 3:
							if (--nearX < 0)
							{
								nearX = VoxelConstants.ChunkAxisCount - 1;
								isSolidData = _isSolidLeft;
							}

							break;
						case 4:
							if (--nearY < 0)
							{
								nearY = VoxelConstants.ChunkAxisCount - 1;
								isSolidData = _isSolidDown;
							}

							break;
						case 5:
							if (++nearY >= VoxelConstants.ChunkAxisCount)
							{
								nearY = 0;
								isSolidData = _isSolidUp;
							}

							break;
					}

					var nearXWeight = nearX << (VoxelConstants.ChunkAxisExponent << 1);
					var nearYWeight = nearY << VoxelConstants.ChunkAxisExponent;
					var nearZWeight = nearZ;

					if (isSolidData[nearXWeight | nearYWeight | nearZWeight])
					{
						continue;
					}

					var blockSideId = (blockId << 3) | s;

					if (!ChunkService.UvInfo.TryGetValue(blockSideId, out var uvInfo))
					{
						continue;
					}

					// 카운트가 모자라다면 Doubling
					if (vertices.Capacity <= vertices.Length)
					{
						vertices.Capacity <<= 1;
						uvs.Capacity <<= 1;
						triangles.Capacity <<= 1;
					}

					// FIXME : 더 줄일 수 있을지도...?
					for (int i = 0; i < VoxelConstants.VertexInSideCount; i++)
					{
						vertices.Add(VoxelConstants.VoxelVerts[VoxelConstants.VoxelTris[s, i]] + pos);

						var uvNormal = VoxelConstants.VoxelUvs[i];

						var uv = new Vector2(uvInfo.startX + uvNormal.x * uvInfo.width,
							uvInfo.startY + uvNormal.y * uvInfo.height);

						uvs.Add(uv);
					}

					// 시계방향으로 그려준다.
					triangles.Add(_triangleVertexWalker);
					triangles.Add(_triangleVertexWalker + 1);
					triangles.Add(_triangleVertexWalker + 2);
					triangles.Add(_triangleVertexWalker + 2);
					triangles.Add(_triangleVertexWalker + 1);
					triangles.Add(_triangleVertexWalker + 3);

					_triangleVertexWalker += VoxelConstants.VertexInSideCount;
				}
			}
		}

		private void Awake()
		{
			_meshFilter = GetComponent<MeshFilter>();
			_meshRenderer = GetComponent<MeshRenderer>();

			_blockIdMap = new int[VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount];

			_isSolidMap = new bool[VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount];
		}

		public void Initialize(Vector3Int coord)
		{
			Coord = coord;

			// FIXME : 테스트용 코드
			if (Coord.y >= 0)
			{
				_chunkType = null;
			}
			else if (Coord.y == -1)
			{
				_chunkType = "grass_dirt";
			}
			else
			{
				_chunkType = "dirt";
			}

			for (int y = 0; y < VoxelConstants.ChunkAxisCount; y++)
			{
				var yWeight = y << VoxelConstants.ChunkAxisExponent;

				for (int x = 0; x < VoxelConstants.ChunkAxisCount; x++)
				{
					var xWeight = x << (VoxelConstants.ChunkAxisExponent << 1);

					for (int z = 0; z < VoxelConstants.ChunkAxisCount; z++)
					{
						var index = xWeight | yWeight | z;

						if (_chunkType == "grass_dirt" && y == VoxelConstants.ChunkAxisCount - 1)
						{
							_blockIdMap[index] = 1;
						}
						else if (_chunkType == null)
						{
							_blockIdMap[index] = -1;
						}
						else
						{
							_blockIdMap[index] = 2;
						}

						_isSolidMap[index] = _blockIdMap[index] >= 0;
					}
				}
			}
		}

		public void RebuildMesh(bool[] isSolidLeft, bool[] isSolidRight, bool[] isSolidUp,
			bool[] isSolidDown, bool[] isSolidForward, bool[] isSolidBack)
		{
			_meshRenderer.enabled = false;

			if (_chunkType == null)
			{
				return;
			}

			_currentBuildingMeshJob = new BuildingMeshJob(_blockIdMap, _isSolidMap,
				isSolidLeft, isSolidRight, isSolidUp, isSolidDown, isSolidForward, isSolidBack);

			if (_currentBuildingMeshJob != null)
			{
				_currentBuildingMeshJobHandler = _currentBuildingMeshJob.Value.Schedule();
			}
		}

		public bool UpdateBuildMesh()
		{
			if (_currentBuildingMeshJobHandler == null || !_currentBuildingMeshJobHandler.Value.IsCompleted ||
			    _currentBuildingMeshJob == null)
			{
				return false;
			}

			_currentBuildingMeshJobHandler.Value.Complete();

			var mesh = new Mesh
			{
				vertices = _currentBuildingMeshJob.Value.vertices.ToArray(),
				triangles = _currentBuildingMeshJob.Value.triangles.ToArray(),
				uv = _currentBuildingMeshJob.Value.uvs.ToArray(),
			};

			_currentBuildingMeshJob.Value.Dispose();

			_currentBuildingMeshJob = null;
			_currentBuildingMeshJobHandler = null;

			mesh.RecalculateNormals();

			_meshFilter.mesh = mesh;

			_meshRenderer.enabled = true;

			return true;
		}

		public bool IsSolidAt(int x, int y, int z)
		{
			var xWeight = x << (VoxelConstants.ChunkAxisExponent << 1);
			var yWeight = y << VoxelConstants.ChunkAxisExponent;

			return _isSolidMap[xWeight | yWeight | z];
		}
	}
}
