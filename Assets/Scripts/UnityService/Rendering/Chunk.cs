using System.Collections.Generic;
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

		private List<Vector3> _vertices;
		private List<int> _triangles;
		private List<Vector2> _uvs;
		private string[,,] _blockMap;

		public GameObject GameObject => gameObject;

		public Vector3Int Coord { get; private set; }

		private bool[] _isSolidMap;

		public bool[] IsSolidMap => _isSolidMap;

		// FIXME : 테스트용
		private string _chunkType;

		private void Awake()
		{
			_meshFilter = GetComponent<MeshFilter>();
			_meshRenderer = GetComponent<MeshRenderer>();

			_blockMap = new string[VoxelConstants.ChunkAxisCount, VoxelConstants.ChunkAxisCount, VoxelConstants.ChunkAxisCount];

			_isSolidMap = new bool[VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount];

			// Capacity를 미리 크게 잡아둠
			var normalSideCount =
				VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.BlockSideCount;

			_vertices = new(normalSideCount * 4);
			_uvs = new(normalSideCount * 4);
			_triangles = new(normalSideCount * 2);
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
						if (_chunkType == "grass_dirt" && y < VoxelConstants.ChunkAxisCount - 1)
						{
							_blockMap[x, y, z] = "dirt";
						}
						else
						{
							_blockMap[x, y, z] = _chunkType;
						}

						_isSolidMap[xWeight | yWeight | z] = _blockMap[x, y, z] != null;

						// _blockMap[x, y, z] = (y % VoxelConstants.ChunkAxisCount < x || y % VoxelConstants.ChunkAxisCount < z) ?
						// 	"dirt": null;
					}
				}
			}
		}

		public void RebuildMesh()
		{
			if (_chunkType == null)
			{
				_meshRenderer.enabled = false;
				return;
			}

			_meshRenderer.enabled = true;

			if (!ServiceManager.TryGetService<IChunkService>(out var chunkService))
			{
				return;
			}

			var vertexIndex = 0;

			for (int y = 0; y < VoxelConstants.ChunkAxisCount; y++)
			{
				for (int x = 0; x < VoxelConstants.ChunkAxisCount; x++)
				{
					for (int z = 0; z < VoxelConstants.ChunkAxisCount; z++)
					{
						if (_blockMap[x, y, z] != null)
						{
							AddVoxelData(chunkService, x, y, z, ref vertexIndex);
						}
					}
				}
			}

			var mesh = new Mesh
			{
				vertices = _vertices.ToArray(),
				triangles = _triangles.ToArray(),
				uv = _uvs.ToArray(),
			};

			// FIXME : 매번 뺐다 넣었다 하지 않고, 해당 블록의 Vertex / Triangle / Uv 위치를 알 방법이 있을까?
			_vertices.Clear();
			_triangles.Clear();
			_uvs.Clear();

			mesh.RecalculateNormals();

			_meshFilter.mesh = mesh;
		}

		private void AddVoxelData(IChunkService chunkService, int x, int y, int z, ref int vertexIndex)
		{
			var blockName = _blockMap[x, y, z];
			var pos = new Vector3(x, y, z);

			for (int p = 0; p < VoxelConstants.BlockSideCount; p++)
			{
				var nearDir = VoxelConstants.NearVoxels[p];

				if (chunkService.IsSolidAt(this, x + nearDir.x, y + nearDir.y, z + nearDir.z))
				{
					continue;
				}

				if (!chunkService.TryGetUvInfo(blockName, p, out var uvInfo))
				{
					continue;
				}

				// 카운트가 모자라다면 Doubling
				if (_vertices.Capacity <= _vertices.Count)
				{
					_vertices.Capacity <<= 1;
					_uvs.Capacity <<= 1;
					_triangles.Capacity <<= 1;
				}

				// FIXME : 더 줄일 수 있을지도...?
				for (int i = 0; i < VoxelConstants.VertexInSideCount; i++)
				{
					_vertices.Add(VoxelConstants.VoxelVerts[VoxelConstants.VoxelTris[p, i]] + pos);

					var uvNormal = VoxelConstants.VoxelUvs[i];

					var uv = new Vector2(uvInfo.startX + uvNormal.x * uvInfo.width,
						uvInfo.startY + uvNormal.y * uvInfo.height);

					_uvs.Add(uv);
				}

				// 시계방향으로 그려준다.
				_triangles.Add(vertexIndex);
				_triangles.Add(vertexIndex + 1);
				_triangles.Add(vertexIndex + 2);
				_triangles.Add(vertexIndex + 2);
				_triangles.Add(vertexIndex + 1);
				_triangles.Add(vertexIndex + 3);

				vertexIndex += VoxelConstants.VertexInSideCount;
			}
		}

		public bool IsSolidAt(int x, int y, int z)
		{
			var xWeight = x << (VoxelConstants.ChunkAxisExponent << 1);
			var yWeight = y << VoxelConstants.ChunkAxisExponent;

			return _isSolidMap[xWeight | yWeight | z];
		}
	}
}
