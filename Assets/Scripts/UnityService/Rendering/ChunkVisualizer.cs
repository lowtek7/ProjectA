using System;
using Game.World.Stage;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UnityService.Rendering
{
	public class ChunkVisualizer : MonoBehaviour, IDisposable
	{
		private MeshFilter _meshFilter;
		private MeshRenderer _meshRenderer;

		private BuildingMeshJob? _currentBuildingMeshJob;
		private JobHandle? _currentBuildingMeshJobHandler;

		private Vector3[] _verticesPool;
		private int[] _trianglesPool;
		private Vector2[] _uvsPool;

		public bool IsBuilding => _currentBuildingMeshJob != null;

		private struct BuildingMeshJob : IJob
		{
			public NativeArray<Vector3> vertices;
			public NativeArray<int> triangles;
			public NativeArray<Vector2> uvs;

			public NativeArray<int> meshDataCounts;

			private NativeArray<ushort> _blockMap;

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

			private int _verticesIndexWalker;
			private int _trianglesIndexWalker;
			private int _uvsIndexWalker;

			public BuildingMeshJob(ushort[] blockIdMap, bool[] isSolidSelf, bool[] isSolidRight, bool[] isSolidLeft,
				bool[] isSolidUp, bool[] isSolidDown, bool[] isSolidForward, bool[] isSolidBack)
			{
				var lifeType = Allocator.TempJob;

				_isSolidSelf = new NativeArray<bool>(isSolidSelf, lifeType);
				_isSolidRight = new NativeArray<bool>(isSolidRight, lifeType);
				_isSolidLeft = new NativeArray<bool>(isSolidLeft, lifeType);
				_isSolidUp = new NativeArray<bool>(isSolidUp, lifeType);
				_isSolidDown = new NativeArray<bool>(isSolidDown, lifeType);
				_isSolidForward = new NativeArray<bool>(isSolidForward, lifeType);
				_isSolidBack = new NativeArray<bool>(isSolidBack, lifeType);

				// Capacity를 미리 크게 잡아둠
				var normalSideCount = ChunkConstants.MaxLocalBlockCount * ChunkConstants.BlockSideCount / 2;

				vertices = new NativeArray<Vector3>(normalSideCount * 4, lifeType);
				triangles = new NativeArray<int>(normalSideCount * 6, lifeType);
				uvs = new NativeArray<Vector2>(normalSideCount * 4, lifeType);

				// Vertex, Uv, Triangle 순서로 저장될 것
				meshDataCounts = new NativeArray<int>(3, lifeType);

				_triangleVertexWalker = 0;
				_verticesIndexWalker = 0;
				_trianglesIndexWalker = 0;
				_uvsIndexWalker = 0;

				_blockMap = new NativeArray<ushort>(blockIdMap, lifeType);
			}

			public void Dispose()
			{
				vertices.Dispose();
				triangles.Dispose();
				uvs.Dispose();
				meshDataCounts.Dispose();

				_blockMap.Dispose();

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
				for (int y = 0; y < ChunkConstants.LocalBlockAxisCount; y++)
				{
					for (int x = 0; x < ChunkConstants.LocalBlockAxisCount; x++)
					{
						for (int z = 0; z < ChunkConstants.LocalBlockAxisCount; z++)
						{
							var localBlockId = ChunkUtility.GetLocalBlockId(x, y, z);

							if (_blockMap[localBlockId] != ChunkConstants.InvalidBlockId) {
								AddBlock(x, y, z, _blockMap[localBlockId]);
							}
						}
					}
				}

				meshDataCounts[0] = _verticesIndexWalker;
				meshDataCounts[1] = _uvsIndexWalker;
				meshDataCounts[2] = _trianglesIndexWalker;
			}

			private void AddBlock(int x, int y, int z, int blockId)
			{
				var pos = new Vector3(x, y, z);

				for (int s = 0; s < ChunkConstants.BlockSideCount; s++)
				{
					var nearX = x;
					var nearY = y;
					var nearZ = z;
					NativeArray<bool> isSolidData = _isSolidSelf;

					switch (s)
					{
						case 0:
							if (++nearX >= ChunkConstants.LocalBlockAxisCount)
							{
								nearX = 0;
								isSolidData = _isSolidRight;
							}

							break;
						case 1:
							if (--nearX < 0)
							{
								nearX = ChunkConstants.LocalBlockAxisCount - 1;
								isSolidData = _isSolidLeft;
							}

							break;
						case 2:
							if (++nearY >= ChunkConstants.LocalBlockAxisCount)
							{
								nearY = 0;
								isSolidData = _isSolidUp;
							}

							break;
						case 3:
							if (--nearY < 0)
							{
								nearY = ChunkConstants.LocalBlockAxisCount - 1;
								isSolidData = _isSolidDown;
							}

							break;
						case 4:
							if (++nearZ >= ChunkConstants.LocalBlockAxisCount)
							{
								nearZ = 0;
								isSolidData = _isSolidForward;
							}

							break;
						case 5:
							if (--nearZ < 0)
							{
								nearZ = ChunkConstants.LocalBlockAxisCount - 1;
								isSolidData = _isSolidBack;
							}

							break;
					}

					var nearXWeight = nearX << (ChunkConstants.LocalBlockAxisExponent << 1);
					var nearYWeight = nearY << ChunkConstants.LocalBlockAxisExponent;
					var nearZWeight = nearZ;

					// if (isSolidData == _isSolidSelf)
					// {
						if (isSolidData[nearXWeight | nearYWeight | nearZWeight])
						{
							continue;
						}
					// }

					var blockSideId = (blockId << 3) | s;

					if (!ChunkRenderService.UvInfo.TryGetValue(blockSideId, out var uvInfo))
					{
						continue;
					}

					// FIXME : 더 줄일 수 있을지도...?
					for (int i = 0; i < VoxelConstants.VertexInSideCount; i++)
					{
						vertices[_verticesIndexWalker++] = VoxelConstants.VoxelVerts[VoxelConstants.VoxelTris[s, i]] + pos;

						var uvNormal = VoxelConstants.VoxelUvs[i];

						var uv = new Vector2(uvInfo.startX + uvNormal.x * uvInfo.width,
							uvInfo.startY + uvNormal.y * uvInfo.height);

						uvs[_uvsIndexWalker++] = uv;
					}

					// 시계방향으로 그려준다.
					triangles[_trianglesIndexWalker++] = _triangleVertexWalker;
					triangles[_trianglesIndexWalker++] = _triangleVertexWalker + 1;
					triangles[_trianglesIndexWalker++] = _triangleVertexWalker + 2;
					triangles[_trianglesIndexWalker++] = _triangleVertexWalker + 2;
					triangles[_trianglesIndexWalker++] = _triangleVertexWalker + 1;
					triangles[_trianglesIndexWalker++] = _triangleVertexWalker + 3;

					_triangleVertexWalker += VoxelConstants.VertexInSideCount;
				}
			}
		}

		private void Awake()
		{
			_meshFilter = GetComponent<MeshFilter>();
			_meshRenderer = GetComponent<MeshRenderer>();

			// Capacity를 미리 크게 잡아둠
			var normalSideCount =
				ChunkConstants.LocalBlockAxisCount * ChunkConstants.LocalBlockAxisCount * ChunkConstants.BlockSideCount;

			_verticesPool = new Vector3[normalSideCount * 4];
			_trianglesPool = new int[normalSideCount * 6];
			_uvsPool = new Vector2[normalSideCount * 4];
		}

		private void OnDestroy()
		{
			Dispose();

			_meshFilter = null;
			_meshRenderer = null;

			_verticesPool = null;
			_trianglesPool = null;
			_uvsPool = null;
		}

		public void Initialize()
		{
			_meshRenderer.enabled = false;
		}

		public void Dispose()
		{
			_meshRenderer.enabled = false;

			StopBuildMesh();
		}

		public void RebuildMesh(ushort[] blockIdMap, bool[] isSolidSelf,
			bool[] isSolidRight, bool[] isSolidLeft,
			bool[] isSolidUp, bool[] isSolidDown,
			bool[] isSolidForward, bool[] isSolidBack)
		{
			var buildingMeshJob = new BuildingMeshJob(blockIdMap, isSolidSelf,
				isSolidRight, isSolidLeft, isSolidUp, isSolidDown, isSolidForward, isSolidBack);

			_currentBuildingMeshJobHandler = buildingMeshJob.Schedule();
			_currentBuildingMeshJob = buildingMeshJob;
		}

		public void UpdateBuildMesh()
		{
			if (_currentBuildingMeshJobHandler is not { IsCompleted: true } || _currentBuildingMeshJob == null)
			{
				return;
			}

			var buildingMeshJob = _currentBuildingMeshJob.Value;

			_currentBuildingMeshJobHandler.Value.Complete();

			// 비어있지 않은 경우에만 데이터를 넣어줌
			if (buildingMeshJob.meshDataCounts[0] > 0)
			{
				var verticesSeg = CopyNativeArray(buildingMeshJob.vertices, ref _verticesPool, buildingMeshJob.meshDataCounts[0]);
				var uvsSeg = CopyNativeArray(buildingMeshJob.uvs, ref _uvsPool, buildingMeshJob.meshDataCounts[1]);
				var trianglesSeg = CopyNativeArray(buildingMeshJob.triangles, ref _trianglesPool, buildingMeshJob.meshDataCounts[2]);

				// TODO : Array가 새로 할당되었으면 사이즈가 동일하다는 뜻이므로, Segment를 통하지 않고 바로 넣어도 될 것임

				var mesh = new Mesh
				{
					vertices = verticesSeg.ToArray(),
					triangles = trianglesSeg.ToArray(),
					uv = uvsSeg.ToArray()
				};

				mesh.RecalculateNormals();
				_meshFilter.mesh = mesh;
				_meshRenderer.enabled = true;
			}

			buildingMeshJob.Dispose();

			_currentBuildingMeshJob = null;
			_currentBuildingMeshJobHandler = null;
		}

		public void StopBuildMesh()
		{
			_currentBuildingMeshJobHandler?.Complete();
			_currentBuildingMeshJob?.Dispose();

			_currentBuildingMeshJob = null;
			_currentBuildingMeshJobHandler = null;
		}

		private ArraySegment<T> CopyNativeArray<T>(NativeArray<T> from, ref T[] to, int count) where T : unmanaged
		{
			var length = from.Length;

			if (to.Length < length)
			{
				to = new T[length];
			}

			var slice = new NativeSlice<T>(from, 0, count);

			RAMGUnsafe.UnsafeUtility.CopyToFast(slice, to);

			var segment = new ArraySegment<T>(to, 0, count);

			return segment;
		}
	}
}
