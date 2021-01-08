using System.Collections.Generic;
using System.Linq;
using Terrain.Voxel;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Terrain.Mesh {
    public struct ChunkGenerateMeshJob : IJob {
        public NativeList<int> triangles;
        public NativeList<Vector3> vertices;

        [ReadOnly] public NativeList<VoxelData> voxels;

        public void Execute() {
            var (verts, tris) = VoxelRender.GenerateVoxelsMesh(new List<VoxelData>(voxels.ToArray()));
            triangles.CopyFrom(tris.ToArray());
            vertices.CopyFrom(verts.ToArray());
        }
    }
}