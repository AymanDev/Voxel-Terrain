using System.Collections.Generic;
using System.Linq;
using Terrain.Voxel;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Terrain.Mesh
{
    public struct ChunkGenerateMeshJob : IJob
    {
        public NativeList<int> triangles;
        public NativeList<Vector3> vertices;

        [ReadOnly] public int chunkSize;
        [ReadOnly] public int chunkHeight;
        [ReadOnly] public NativeList<VoxelData> voxels;

        public void Execute()
        {

            var marching = new MarchingCubes(0f);
            // var verts = new List<Vector3>();
            // var inds = new List<int>();
            // marching.Generate(new List<VoxelData>(voxels.ToArray()), chunkSize, chunkHeight,
            //     chunkSize);

            // vertices.CopyFrom(verts.ToArray());
            // triangles.CopyFrom(inds.ToArray());
        }
    }
}
