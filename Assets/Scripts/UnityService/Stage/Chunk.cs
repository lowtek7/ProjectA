using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityService.Stage
{
	public enum BlockId
	{
		Empty = 0,
		Dirt = 1,
	}

	public class Chunk : MonoBehaviour
	{
		private MeshRenderer _meshRenderer;
		private MeshFilter _meshFilter;

		List<Vector3> vertices = new();
		List<int> triangles = new();
		List<Vector2> uvs = new();

		private BlockId[,,] voxelMap;

		private void Awake()
		{
			_meshFilter = GetComponent<MeshFilter>();
			_meshRenderer = GetComponent<MeshRenderer>();

			voxelMap = new BlockId[VoxelConstants.ChunkWidth, VoxelConstants.ChunkHeight, VoxelConstants.ChunkWidth];

			for (int y = 0; y < voxelMap.GetLength(1); y++)
			{
				for (int x = 0; x < voxelMap.GetLength(0); x++)
				{
					for (int z = 0; z < voxelMap.GetLength(2); z++)
					{
						voxelMap[x, y, z] = (y % VoxelConstants.ChunkWidth < x || y % VoxelConstants.ChunkWidth < z) ?
							BlockId.Dirt : BlockId.Empty;
					}
				}
			}

			RebuildMesh();
		}

		public void RebuildMesh()
		{
			int vertexIndex = 0;

			for (int y = 0; y < voxelMap.GetLength(1); y++)
			{
				for (int x = 0; x < voxelMap.GetLength(0); x++)
				{
					for (int z = 0; z < voxelMap.GetLength(2); z++)
					{
						var pos = new Vector3Int(x, y, z);

						if (IsSolidAt(pos))
						{
							vertexIndex = AddVoxelData(pos, vertexIndex);
						}
					}
				}
			}

			var mesh = new Mesh
			{
				vertices = vertices.ToArray(),
				triangles = triangles.ToArray(),
				uv = uvs.ToArray(),
			};

			vertices.Clear();
			triangles.Clear();
			uvs.Clear();

			mesh.RecalculateNormals();

			_meshFilter.mesh = mesh;
		}

		private int AddVoxelData(Vector3Int pos, int vertexIndex)
		{
			var sideCount = VoxelConstants.VoxelTris.GetLength(0);
			var vertexInRectCount = VoxelConstants.VoxelTris.GetLength(1);

			for (int p = 0; p < sideCount; p++)
			{
				if (IsSolidAt(pos + VoxelConstants.NearVoxels[p]))
				{
					continue;
				}

				// FIXME : 더 줄일 수 있을지도...?
				for (int i = 0; i < vertexInRectCount; i++)
				{
					vertices.Add(VoxelConstants.VoxelVerts[VoxelConstants.VoxelTris[p, i]] + pos);
					uvs.Add(VoxelConstants.VoxelUvs[i]);
				}

				// 시계방향으로 그려준다.
				triangles.Add(vertexIndex);
				triangles.Add(vertexIndex + 1);
				triangles.Add(vertexIndex + 2);
				triangles.Add(vertexIndex + 2);
				triangles.Add(vertexIndex + 1);
				triangles.Add(vertexIndex + 3);

				vertexIndex += vertexInRectCount;
			}

			return vertexIndex;
		}

		private bool IsSolidAt(Vector3Int pos)
		{
			if (pos.x < 0 || pos.y < 0 || pos.z < 0 ||
			    pos.x >= voxelMap.GetLength(0) ||
			    pos.y >= voxelMap.GetLength(1) ||
			    pos.z >= voxelMap.GetLength(2))
			{
				return false;
			}

			return voxelMap[pos.x, pos.y, pos.z] != BlockId.Empty;
		}
	}
}
