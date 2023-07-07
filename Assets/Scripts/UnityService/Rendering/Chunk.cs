﻿using System;
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

		public bool NeedWaitBuildMesh => _chunkType > 0;

		// FIXME : 테스트용
		private int _chunkType;

		private BuildingMeshJob? _currentBuildingMeshJob;
		private JobHandle? _currentBuildingMeshJobHandler;

		private Vector3[] _verticesPool;
		private int[] _trianglesPool;
		private Vector2[] _uvsPool;

		private struct BuildingMeshJob : IJob
		{
			public NativeArray<Vector3> vertices;
			public NativeArray<int> triangles;
			public NativeArray<Vector2> uvs;

			public NativeArray<int> blockMap;

			public NativeArray<int> meshDataCounts;

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
				var normalSideCount = VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount *
					VoxelConstants.BlockSideCount / 2;

				vertices = new NativeArray<Vector3>(normalSideCount * 4, lifeType);
				triangles = new NativeArray<int>(normalSideCount * 2, lifeType);
				uvs = new NativeArray<Vector2>(normalSideCount * 4, lifeType);

				// Vertex, Uv, Triangle 순서로 저장될 것
				meshDataCounts = new NativeArray<int>(3, lifeType);

				_triangleVertexWalker = 0;
				_verticesIndexWalker = 0;
				_trianglesIndexWalker = 0;
				_uvsIndexWalker = 0;

				blockMap = new NativeArray<int>(blockIdMap, lifeType);
			}

			public void Dispose()
			{
				vertices.Dispose();
				triangles.Dispose();
				uvs.Dispose();
				meshDataCounts.Dispose();

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

				meshDataCounts[0] = _verticesIndexWalker;
				meshDataCounts[1] = _uvsIndexWalker;
				meshDataCounts[2] = _trianglesIndexWalker;
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

			_blockIdMap = new int[VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount];

			_isSolidMap = new bool[VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount];

			// Capacity를 미리 크게 잡아둠
			var normalSideCount =
				VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.BlockSideCount;

			_verticesPool = new Vector3[normalSideCount * 4];
			_trianglesPool = new int[normalSideCount * 2];
			_uvsPool = new Vector2[normalSideCount * 4];
		}

		public void Initialize(Vector3Int coord)
		{
			Coord = coord;

			// FIXME : 테스트용 코드
			if (Coord.y >= 0)
			{
				_chunkType = -1;
			}
			else if (Coord.y == -1)
			{
				_chunkType = 1;
			}
			else
			{
				_chunkType = 2;
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

						if (_chunkType == 1 && y == VoxelConstants.ChunkAxisCount - 1)
						{
							_blockIdMap[index] = 1;
						}
						else if (!NeedWaitBuildMesh)
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

			if (!NeedWaitBuildMesh)
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
			if (_chunkType == null)
			{
				return true;
			}

			if (_currentBuildingMeshJobHandler == null || !_currentBuildingMeshJobHandler.Value.IsCompleted ||
			    _currentBuildingMeshJob == null)
			{
				return false;
			}

			var buildingMeshJob = _currentBuildingMeshJob.Value;

			_currentBuildingMeshJobHandler.Value.Complete();

			var verticesSeg = CopyNativeArray(buildingMeshJob.vertices, ref _verticesPool, buildingMeshJob.meshDataCounts[0]);
			var uvsSeg = CopyNativeArray(buildingMeshJob.uvs, ref _uvsPool, buildingMeshJob.meshDataCounts[1]);
			var trianglesSeg = CopyNativeArray(buildingMeshJob.triangles, ref _trianglesPool, buildingMeshJob.meshDataCounts[2]);

			var mesh = new Mesh
			{
				vertices = verticesSeg.Array,
				triangles = trianglesSeg.Array,
				uv = uvsSeg.Array
			};

			buildingMeshJob.Dispose();

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

		private ArraySegment<T> CopyNativeArray<T>(NativeArray<T> from, ref T[] to, int count) where T : unmanaged
		{
			var length = from.Length;

			if (to.Length < length)
			{
				to = new T[length];
			}

			var slice = new NativeSlice<T>(from, 0, count);

			RAMGUnsafe.UnsafeUtility.CopyToFast(slice, to);

			var segment = new ArraySegment<T>(to, 0, length);

			return segment;
		}
	}
}
