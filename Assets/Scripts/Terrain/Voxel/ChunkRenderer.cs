using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terrain.Mesh;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Terrain.Voxel
{
    [Serializable]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class ChunkRenderer : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider meshCollider;

        // public UnityEngine.Mesh Mesh => meshFilter.mesh;
        //
        // private JobHandleExtended _jobHandle;
        // private NativeList<int> _triangles;
        // private NativeList<Vector3> _vertices;
        // private NativeList<VoxelData> _voxels;

        private Task<Tuple<List<Vector3>, List<int>>> _meshTask;

        public bool IsRenderReady { get; private set; }

        public void ResetRender()
        {
            IsRenderReady = false;
        }

        public void StartMeshTask(ChunkData chunkData)
        {
            var position = transform.localPosition;
            ResetRender();
            _meshTask = Task.Run(() =>
            {
                var terrain = TerrainManager.instance;
                var marching = new MarchingCubes(0f);
                return marching.Generate(terrain, terrain.config.chunkSize, terrain.config.chunkHeight,
                    terrain.config.chunkSize, position);
            });
        }

        private void LateUpdate()
        {
            if (_meshTask == null || IsRenderReady)
            {
                return;
            }

            if (!_meshTask.IsCompleted)
            {
                return;
            }

            var (verts, tris) = _meshTask.Result;
            UpdateMesh(verts, tris);
            IsRenderReady = true;
        }

        // private void LateUpdate()
        // {
        //     if (!_triangles.IsCreated || !_vertices.IsCreated)
        //     {
        //         return;
        //     }
        //
        //     if (_jobHandle.Status != JobHandleStatus.AwaitingCompletion)
        //         return;
        //     _jobHandle.Complete();
        //
        //     _voxels.Dispose();
        //
        //     UpdateMesh(new List<Vector3>(_vertices.ToArray()), new List<int>(_triangles.ToArray()));
        //     _vertices.Dispose();
        //     _triangles.Dispose();
        //
        //     IsRenderReady = true;
        // }

        public void UpdateMesh(List<Vector3> verts, List<int> tris)
        {
            var mesh = meshFilter.mesh;
            mesh.Clear();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            // mesh.vertices = verts.ToArray();
            // mesh.triangles = tris.ToArray();
            // mesh.SetVertices(verts.ToArray());
            // mesh.SetTriangles(tris.ToArray(), 0);
            // mesh.SetIndices(tris.ToArray(), MeshTopology.Triangles, 0);
            // mesh.triangles = tris.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            // mesh.RecalculateTangents();
            // mesh.RecalculateUVDistributionMetrics();

            meshFilter.mesh = mesh;
            // meshCollider.sharedMesh = Mesh;
        }

        public static (List<Vector3> vertices, ICollection<int> triangles) GenerateVoxelMesh(List<VoxelData> voxels)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();

            foreach (var voxelData in voxels)
            {
                MakeCube(voxelData.X, voxelData.Y, voxelData.Z, voxels, verts, tris);
            }

            return (verts, tris);
        }

        public static void MakeCube(int x,
            int y,
            int z,
            ChunkData data,
            List<Vector3> vertices,
            ICollection<int> triangles)
        {
            var position = new Vector3(x, y, z);
            for (var i = 0; i < 6; i++)
            {
                var dir = (Direction)i;
                if (data.GetNeighbor(x, y, z, dir) == 0)
                {
                    MakeFace(dir, position, vertices, triangles);
                }
            }
        }

        public static void MakeCube(int x,
            int y,
            int z,
            List<VoxelData> voxels,
            List<Vector3> vertices,
            ICollection<int> triangles)
        {
            var position = new Vector3(x, y, z);
            for (var i = 0; i < 6; i++)
            {
                var dir = (Direction)i;
                if (ChunkData.GetNeighbor(voxels, x, y, z, dir) == 0)
                {
                    MakeFace(dir, position, vertices, triangles);
                }
            }
        }

        private static void MakeFace(Direction dir,
            Vector3 position,
            List<Vector3> vertices,
            ICollection<int> triangles)
        {
            vertices.AddRange(BasicMeshData.CubeMeshData.FaceVertices(dir, position));
            var vCount = vertices.Count;

            triangles.Add(vCount - 4);
            triangles.Add(vCount - 4 + 1);
            triangles.Add(vCount - 4 + 2);
            triangles.Add(vCount - 4);
            triangles.Add(vCount - 4 + 2);
            triangles.Add(vCount - 4 + 3);
        }
    }
}
