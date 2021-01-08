using System;
using System.Collections.Generic;
using System.Linq;
using Terrain.Mesh;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Terrain.Voxel {
    [Serializable]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class VoxelRender : MonoBehaviour {
        [SerializeField] private MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider meshCollider;

        public UnityEngine.Mesh Mesh => meshFilter.mesh;

        private JobHandleExtended _jobHandle;
        private NativeList<int> _triangles;
        private NativeList<Vector3> _vertices;
        private NativeList<VoxelData> _voxels;

        public bool IsRenderReady { get; private set; }

        public void ResetRender() {
            IsRenderReady = false;
        }

        public void GenerateMesh(ChunkData chunkData) {
            ResetRender();

            _triangles = new NativeList<int>(Allocator.TempJob);
            _vertices = new NativeList<Vector3>(Allocator.TempJob);

            _voxels = new NativeList<VoxelData>(chunkData.Voxels.Count, Allocator.TempJob);
            _voxels.CopyFrom(chunkData.Voxels.ToArray());

            var job = new ChunkGenerateMeshJob() {
                triangles = _triangles,
                vertices = _vertices,
                voxels = _voxels
            };

            _jobHandle = new JobHandleExtended(job.Schedule());
        }

        private void Update() {
            if (!_triangles.IsCreated || !_vertices.IsCreated) {
                return;
            }

            if (_jobHandle.Status == JobHandleStatus.AwaitingCompletion) {
                _jobHandle.Complete();

                _voxels.Dispose();

                UpdateMesh(_vertices.ToArray(), _triangles.ToArray());
                _vertices.Dispose();
                _triangles.Dispose();

                IsRenderReady = true;
            }
        }

        public void UpdateMesh(IEnumerable<Vector3> verts, IEnumerable<int> tris) {
            var mesh = meshFilter.mesh;
            mesh.Clear();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
            // meshCollider.sharedMesh = Mesh;
        }

        public static (List<Vector3> vertices, ICollection<int> triangles) GenerateVoxelsMesh(List<VoxelData> voxels) {
            var verts = new List<Vector3>();
            var tris = new List<int>();

            foreach (var voxelData in voxels) {
                MakeCube(voxelData.x, voxelData.y, voxelData.z, voxels, verts, tris);
            }

            return (verts, tris);
        }

        public static void MakeCube(int x, int y, int z, ChunkData data, List<Vector3> vertices,
            ICollection<int> triangles) {
            var position = new Vector3(x, y, z);
            for (var i = 0; i < 6; i++) {
                var dir = (Direction) i;
                if (data.GetNeighbor(x, y, z, dir) == 0) {
                    MakeFace(dir, position, vertices, triangles);
                }
            }
        }

        public static void MakeCube(int x, int y, int z, List<VoxelData> voxels, List<Vector3> vertices,
            ICollection<int> triangles) {
            var position = new Vector3(x, y, z);
            for (var i = 0; i < 6; i++) {
                var dir = (Direction) i;
                if (ChunkData.GetNeighbor(voxels, x, y, z, dir) == 0) {
                    MakeFace(dir, position, vertices, triangles);
                }
            }
        }

        private static void MakeFace(Direction dir, Vector3 position, List<Vector3> vertices,
            ICollection<int> triangles) {
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