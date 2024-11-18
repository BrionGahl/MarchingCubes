using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using MarchingCubes.data;
using MarchingCubes.lookup;

namespace MarchingCubes;

public partial class MarchingCubes : Node
{

	[Export]
	private NoiseTexture3D _noiseTexture;

	private MeshInstance3D _meshInstance;
	private ArrayMesh _am;

	private Array _array;
	private List<Vector3> _vertices;
	private List<Vector2> _uvs;
	private List<Vector3> _normals;
	private List<int> _indices;

	private int _cellSize;
	
	public override void _Ready()
	{
		_meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
		_am = new ArrayMesh();

		_cellSize = 1;
		
		_array = new Array();
		_vertices = new List<Vector3>();
		_uvs = new List<Vector2>();
		_normals = new List<Vector3>();
		_indices = new List<int>();
		
		_array.Resize((int)Mesh.ArrayType.Max);
		for (int x = 0; x < 32; x++)
		for (int y = 0; y < 32; y++)
		for (int z = 0; z < 32; z++)
			HandleCell(new Vector3(x, y, z), -0.6f);

		CalculateNormals();
		
		_array[(int)Mesh.ArrayType.Vertex] = _vertices.ToArray();
		// _array[(int)Mesh.ArrayType.TexUV] = _uvs.ToArray();
		_array[(int)Mesh.ArrayType.Normal] = _normals.ToArray();
		// _array[(int)Mesh.ArrayType.Index] = _indices.ToArray();
		
		_am.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, _array);
		_meshInstance.Mesh = _am;
	}

	// Take a 8 points of a given cube and a threshold, then perform algo on it.
	private void HandleCell(Vector3 pos, float isoLevel)
	{
		Vector3[] vertexArr = new Vector3[12];
		CubeCorner[] cubeCorners = {
			Evaluate(new Vector3(pos.X + 0, pos.Y + 0, pos.Z + 0)), // 0, 0, 0
			Evaluate(new Vector3(pos.X + 1, pos.Y + 0, pos.Z + 0)), // 1, 0, 0
			Evaluate(new Vector3(pos.X + 1, pos.Y + 0, pos.Z + 1)), // 1, 0, 1
			Evaluate(new Vector3(pos.X + 0, pos.Y + 0, pos.Z + 1)), // 0, 0, 1
			Evaluate(new Vector3(pos.X + 0, pos.Y + 1, pos.Z + 0)), // 0, 1, 0
			Evaluate(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 0)), // 1, 1, 0
			Evaluate(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1)), // 1, 1, 1
			Evaluate(new Vector3(pos.X + 0, pos.Y + 1, pos.Z + 1))  // 0, 1, 1
		};

		int cubeIndex = 0;
		if (cubeCorners[0].Value < isoLevel)
			cubeIndex |= 1;
		if (cubeCorners[1].Value < isoLevel) 
			cubeIndex |= 2;
		if (cubeCorners[2].Value < isoLevel) 
			cubeIndex |= 4;
		if (cubeCorners[3].Value < isoLevel) 
			cubeIndex |= 8;
		if (cubeCorners[4].Value < isoLevel)
			cubeIndex |= 16;
		if (cubeCorners[5].Value < isoLevel) 
			cubeIndex |= 32;
		if (cubeCorners[6].Value < isoLevel)
			cubeIndex |= 64;
		if (cubeCorners[7].Value < isoLevel)
			cubeIndex |= 128;
		
		// Check if cube is entirely in / out of mesh
		if (EdgeTable.Table[cubeIndex] == 0)
		{
			return;
		}
		
		
		// Find the vertices where the surface intersects the cube 
		if (IsBitSet(EdgeTable.Table[cubeIndex], 0))
			vertexArr[0] = VertexInterpolate(isoLevel,cubeCorners[0].Pos,cubeCorners[1].Pos,cubeCorners[0].Value,cubeCorners[1].Value);
		if (IsBitSet(EdgeTable.Table[cubeIndex], 1))
			vertexArr[1] = VertexInterpolate(isoLevel,cubeCorners[1].Pos,cubeCorners[2].Pos,cubeCorners[1].Value,cubeCorners[2].Value);
		if (IsBitSet(EdgeTable.Table[cubeIndex], 2))
			vertexArr[2] = VertexInterpolate(isoLevel,cubeCorners[2].Pos,cubeCorners[3].Pos,cubeCorners[2].Value,cubeCorners[3].Value);
		if (IsBitSet(EdgeTable.Table[cubeIndex], 3))
			vertexArr[3] = VertexInterpolate(isoLevel,cubeCorners[3].Pos,cubeCorners[0].Pos,cubeCorners[3].Value,cubeCorners[0].Value);
		if (IsBitSet(EdgeTable.Table[cubeIndex], 4))
			vertexArr[4] = VertexInterpolate(isoLevel,cubeCorners[4].Pos,cubeCorners[5].Pos,cubeCorners[4].Value,cubeCorners[5].Value);
		if (IsBitSet(EdgeTable.Table[cubeIndex], 5))
			vertexArr[5] = VertexInterpolate(isoLevel,cubeCorners[5].Pos,cubeCorners[6].Pos,cubeCorners[5].Value,cubeCorners[6].Value);
		if (IsBitSet(EdgeTable.Table[cubeIndex], 6))
			vertexArr[6] = VertexInterpolate(isoLevel,cubeCorners[6].Pos,cubeCorners[7].Pos,cubeCorners[6].Value,cubeCorners[7].Value);
		if (IsBitSet(EdgeTable.Table[cubeIndex], 7))
			vertexArr[7] = VertexInterpolate(isoLevel,cubeCorners[7].Pos,cubeCorners[4].Pos,cubeCorners[7].Value,cubeCorners[4].Value);
		if (IsBitSet(EdgeTable.Table[cubeIndex], 8))
			vertexArr[8] = VertexInterpolate(isoLevel,cubeCorners[0].Pos,cubeCorners[4].Pos,cubeCorners[0].Value,cubeCorners[4].Value);
		if (IsBitSet(EdgeTable.Table[cubeIndex], 9))
			vertexArr[9] = VertexInterpolate(isoLevel,cubeCorners[1].Pos,cubeCorners[5].Pos,cubeCorners[1].Value,cubeCorners[5].Value);
		if (IsBitSet(EdgeTable.Table[cubeIndex], 10))
			vertexArr[10] = VertexInterpolate(isoLevel,cubeCorners[2].Pos,cubeCorners[6].Pos,cubeCorners[2].Value,cubeCorners[6].Value);
		if (IsBitSet(EdgeTable.Table[cubeIndex], 11))
			vertexArr[11] = VertexInterpolate(isoLevel,cubeCorners[3].Pos,cubeCorners[7].Pos,cubeCorners[3].Value,cubeCorners[7].Value);
		
		/* Create the triangle */
		for (int i = 0; TriTable.Table[cubeIndex, i] != -1; i+=3) {
			_vertices.Add(vertexArr[TriTable.Table[cubeIndex, i]]);
			_vertices.Add(vertexArr[TriTable.Table[cubeIndex, i + 1]]);
			_vertices.Add(vertexArr[TriTable.Table[cubeIndex, i + 2]]);
		}
	}
	
	/*
	 *  Take a position and return a floating point value based on the noice map
	 */
	private CubeCorner Evaluate(Vector3 pos)
	{
		return new CubeCorner(pos, _noiseTexture.Noise.GetNoise3Dv(pos) * 2 - 1);
	}

	private Vector3 VertexInterpolate(float isoLevel, Vector3 pos1, Vector3 pos2, float noise1, float noise2)
	{
		float mu;
		float x, y, z;

		if (Mathf.Abs(isoLevel - noise1) < 0.00001)
			return pos1;
		if (Mathf.Abs(isoLevel - noise2) < 0.00001)
			return pos2;
		if (Mathf.Abs(noise1 - noise2) < 0.00001)
			return pos1;

		mu = (isoLevel - noise1) / (noise2 - noise1);
		x = pos1.X + mu * (pos2.X - pos1.X);
		y = pos1.Y + mu * (pos2.Y - pos1.Y);
		z = pos1.Z + mu * (pos2.Z - pos1.Z);
		return new Vector3(x, y, z);
	}
	
	// Bit Helper
	private bool IsBitSet(int b, int pos)
	{
		return (b & (1 << pos)) != 0;
	}

	private void CalculateNormals()
	{
		Vector3 a;
		Vector3 b;
		Vector3 c;
		for (int i = 0; i < _vertices.Count; i += 3)
		{
			a = _vertices[i];
			b = _vertices[i + 1];
			c = _vertices[i + 2];
			
			_normals.Add((b - a).Cross(c - a).Normalized()); // A
			_normals.Add((c - b).Cross(a - b).Normalized()); // B
			_normals.Add((a - c).Cross(b - c).Normalized()); // C
		}
	}
	
}
