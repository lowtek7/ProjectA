using System;
using System.Collections.Generic;
using DG.DemiEditor;
using UnityEditor;
using UnityEngine;
using UnityService.Texture;

namespace UnityService.Stage
{
	public class Chunk : MonoBehaviour
	{
		private MeshRenderer _meshRenderer;
		private MeshFilter _meshFilter;

		List<Vector3> vertices = new();
		List<int> triangles = new();
		List<Vector2> uvs = new();

		private string[,,] _voxelMap;

		private BlockSpecHolder _blockSpecHolder;
		private PackedTextureUvHolder _uvHolder;

		private void Awake()
		{
			_meshFilter = GetComponent<MeshFilter>();
			_meshRenderer = GetComponent<MeshRenderer>();

			_voxelMap = new string[VoxelConstants.ChunkWidth, VoxelConstants.ChunkHeight, VoxelConstants.ChunkWidth];

			for (int y = 0; y < _voxelMap.GetLength(1); y++)
			{
				for (int x = 0; x < _voxelMap.GetLength(0); x++)
				{
					for (int z = 0; z < _voxelMap.GetLength(2); z++)
					{
						_voxelMap[x, y, z] = (y % VoxelConstants.ChunkWidth < x || y % VoxelConstants.ChunkWidth < z) ?
							"dirt": null;
					}
				}
			}

			for (int y = 0; y < _voxelMap.GetLength(1); y++)
			{
				for (int x = 0; x < _voxelMap.GetLength(0); x++)
				{
					for (int z = 0; z < _voxelMap.GetLength(2); z++)
					{
						// FIXME : 이 규칙은 블록마다 따로 분리 필요
						if (_voxelMap[x, y, z] == "dirt" &&
						    !IsSolidAt(new Vector3Int(x, y, z) + VoxelConstants.NearVoxels[5]))
						{
							_voxelMap[x, y, z] = "grass_dirt";
						}
					}
				}
			}

			// FIXME : 원래는 AssetFactory에서 얻어와야 하지만, 테스트 씬에서 사용할 수 있도록 세팅
			_blockSpecHolder = AssetDatabase.LoadAssetAtPath<BlockSpecHolder>(
				"Assets/GameAsset/ScriptableObject/Texture/BlockSpecs.asset");
			_uvHolder = AssetDatabase.LoadAssetAtPath<PackedTextureUvHolder>(
				"Assets/GameAsset/ScriptableObject/Texture/Uvs/PackedTextureUvs.asset");

			RebuildMesh();
		}

		public void RebuildMesh()
		{
			var vertexIndex = 0;

			for (int y = 0; y < _voxelMap.GetLength(1); y++)
			{
				for (int x = 0; x < _voxelMap.GetLength(0); x++)
				{
					for (int z = 0; z < _voxelMap.GetLength(2); z++)
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
			var blockName = _voxelMap[pos.x, pos.y, pos.z];

			if (!_blockSpecHolder.NameToSpec.TryGetValue(blockName, out var blockSpec))
			{
				return vertexIndex;
			}

			for (int p = 0; p < sideCount; p++)
			{
				if (IsSolidAt(pos + VoxelConstants.NearVoxels[p]))
				{
					continue;
				}

				if (_uvHolder.NameToUvs.TryGetValue(blockSpec.textures[p].name, out var uvData))
				{
					// FIXME : 더 줄일 수 있을지도...?
					for (int i = 0; i < vertexInRectCount; i++)
					{
						vertices.Add(VoxelConstants.VoxelVerts[VoxelConstants.VoxelTris[p, i]] + pos);

						var uv = VoxelConstants.VoxelUvs[i];

						var pixelUv = new Vector2(uvData.startX + uv .x * uvData.originTexture.width,
							uvData.startY + uv.y * uvData.originTexture.height);

						uvs.Add(pixelUv / _uvHolder.textureSize);
					}
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
			    pos.x >= _voxelMap.GetLength(0) ||
			    pos.y >= _voxelMap.GetLength(1) ||
			    pos.z >= _voxelMap.GetLength(2))
			{
				return false;
			}

			return !_voxelMap[pos.x, pos.y, pos.z].IsNullOrEmpty();
		}
	}
}
