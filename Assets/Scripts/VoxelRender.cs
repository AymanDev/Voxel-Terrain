using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class VoxelRender : MonoBehaviour {
    [SerializeField] private MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public Mesh Mesh => meshFilter.mesh;

    public (List<Vector3> vertices, ICollection<int> triangles) GenerateVoxelMesh(VoxelData data) {
        var verts = new List<Vector3>();
        var tris = new List<int>();

        for (var z = 0; z < data.Depth; z++) {
            for (var x = 0; x < data.Width; x++) {
                for (var y = 0; y < data.Height; y++) {
                    if (data.GetVoxel(x, y, z) == 0) {
                        continue;
                    }

                    MakeCube(x, y, z, data, verts, tris);
                }
            }
        }

        return (verts, tris);
    }

    public static void MakeCube(int x, int y, int z, VoxelData data, List<Vector3> vertices,
        ICollection<int> triangles) {
        var position = new Vector3(x, y, z);
        for (var i = 0; i < 6; i++) {
            var dir = (Direction) i;
            if (data.GetNeighbor(x, y, z, dir) == 0) {
                MakeFace(dir, position, vertices, triangles);
            }
        }
    }

    private static void MakeFace(Direction dir, Vector3 position, List<Vector3> vertices, ICollection<int> triangles) {
        vertices.AddRange(BasicMeshData.CubeMeshData.FaceVertices(dir, position));
        var vCount = vertices.Count;

        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 1);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4 + 3);
    }

    public void UpdateMesh(List<Vector3> vertices, IEnumerable<int> triangles) {
        var mesh = meshFilter.mesh;
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}