using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class VoxelRender : MonoBehaviour {
    private Mesh _mesh;
    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _triangles = new List<int>();

    private void Awake() {
        _mesh = GetComponent<MeshFilter>().mesh;
    }


    public void GenerateVoxelMesh(VoxelData data) {
        for (var z = 0; z < data.Depth; z++) {
            for (var x = 0; x < data.Width; x++) {
                for (var y = 0; y < data.Height; y++) {
                    if (data.GetVoxel(x, y, z) == 0) {
                        continue;
                    }

                    MakeCube(x, y, z, data);
                }
            }
        }
    }

    public void MakeCube(int x, int y, int z, VoxelData data) {
        var position = new Vector3(x, y, z);
        for (var i = 0; i < 6; i++) {
            var dir = (Direction) i;
            if (data.GetNeighbor(x, y, z, dir) == 0) {
                MakeFace(dir, position);
            }
        }
    }

    private void MakeFace(Direction dir, Vector3 position) {
        _vertices.AddRange(BasicMeshData.CubeMeshData.FaceVertices(dir, position));
        var vCount = _vertices.Count;

        _triangles.Add(vCount - 4);
        _triangles.Add(vCount - 4 + 1);
        _triangles.Add(vCount - 4 + 2);
        _triangles.Add(vCount - 4);
        _triangles.Add(vCount - 4 + 2);
        _triangles.Add(vCount - 4 + 3);
    }

    public void UpdateMesh() {
        _mesh.Clear();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.RecalculateNormals();
    }
}