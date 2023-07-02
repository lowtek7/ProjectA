using System.Collections.Generic;
using Service;
using Service.Rendering;
using UnityEngine;

namespace UnityService.Rendering
{
	public class Chunk : MonoBehaviour
	{
		private MeshFilter _meshFilter;
		private MeshRenderer _meshRenderer;

		private List<Vector3> _vertices;
		private List<int> _triangles;
		private List<Vector2> _uvs;
		private string[,,] _voxelMap;

		private Vector3Int _coord;

		private bool _isAir = false;

		private void Awake()
		{
			_meshFilter = GetComponent<MeshFilter>();
			_meshRenderer = GetComponent<MeshRenderer>();

			_voxelMap = new string[VoxelConstants.ChunkAxisCount, VoxelConstants.ChunkAxisCount, VoxelConstants.ChunkAxisCount];

			// Capacity를 미리 크게 잡아둠
			var normalSideCount = VoxelConstants.ChunkAxisCount * VoxelConstants.ChunkAxisCount * VoxelConstants.VoxelTris.Length;

			_vertices = new(normalSideCount * 4);
			_uvs = new(normalSideCount * 4);
			_triangles = new(normalSideCount * 2);
		}

		public void Initialize(Vector3Int coord)
		{
			_coord = coord;

			// FIXME : 테스트용 코드
			_isAir = transform.position.y >= 0;

			for (int y = 0; y <VoxelConstants.ChunkAxisCount; y++)
			{
				for (int x = 0; x < VoxelConstants.ChunkAxisCount; x++)
				{
					for (int z = 0; z < VoxelConstants.ChunkAxisCount; z++)
					{
						if (_isAir)
						{
							_voxelMap[x, y, z] = null;
						}
						else
						{
							_voxelMap[x, y, z] = "dirt";
						}

						// _voxelMap[x, y, z] = (y % VoxelConstants.ChunkAxisCount < x || y % VoxelConstants.ChunkAxisCount < z) ?
						// 	"dirt": null;
					}
				}
			}
		}

		public void RebuildMesh()
		{
			if (_isAir)
			{
				_meshRenderer.enabled = false;
				return;
			}

			_meshRenderer.enabled = true;

			// FIXME : 임시 처리

			for (int y = 0; y < VoxelConstants.ChunkAxisCount; y++)
			{
				for (int x = 0; x < VoxelConstants.ChunkAxisCount; x++)
				{
					for (int z = 0; z < VoxelConstants.ChunkAxisCount; z++)
					{
						// FIXME : 이 규칙은 블록마다 따로 분리하고 로직을 위쪽(Service)으로 빼야 함
						if (_voxelMap[x, y, z] == "dirt" && !IsSolidAt(x, y + 1, z))
						{
							_voxelMap[x, y, z] = "grass_dirt";
						}
					}
				}
			}

			var vertexIndex = 0;

			for (int y = 0; y < VoxelConstants.ChunkAxisCount; y++)
			{
				for (int x = 0; x < VoxelConstants.ChunkAxisCount; x++)
				{
					for (int z = 0; z < VoxelConstants.ChunkAxisCount; z++)
					{
						if (IsSolidAt(x, y, z))
						{
							AddVoxelData(x, y, z, ref vertexIndex);
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
		private void AddVoxelData(int x, int y, int z, ref int vertexIndex)
		{
			if (!ServiceManager.TryGetService<IChunkService>(out var chunkService))
			{
				return;
			}

			var blockName = _voxelMap[x, y, z];
			var pos = new Vector3(x, y, z);

			for (int p = 0; p < VoxelConstants.BlockSideCount; p++)
			{
				var nearDir = VoxelConstants.NearVoxels[p];

				if (IsSolidAt(x + nearDir.x, y + nearDir.y, z + nearDir.z))
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
			if (x < 0 || y < 0 || z < 0 ||
			    x >= VoxelConstants.ChunkAxisCount ||
			    y >= VoxelConstants.ChunkAxisCount ||
			    z >= VoxelConstants.ChunkAxisCount)
			{
				if (ServiceManager.TryGetService<IChunkService>(out var chunkService))
				{
					return chunkService.IsSolidAt(_coord, x, y, z);
				}

				return false;
			}

			return _voxelMap[x, y, z] != null;
		}
	}
}
