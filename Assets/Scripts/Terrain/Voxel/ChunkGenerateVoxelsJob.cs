using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Terrain.Voxel {
    public struct ChunkGenerateVoxelsJob : IJob {
        public NativeList<VoxelData> voxelData;
        [ReadOnly] public float noiseScale;
        [ReadOnly] public int chunkSize;
        [ReadOnly] public int chunkHeight;
        [ReadOnly] public Vector3 position;

        public void Execute() {
            for (var x = 0; x < chunkSize; x++) {
                for (var z = 0; z < chunkSize; z++) {
                    var vX = Mathf.RoundToInt(position.x + x);
                    var vZ = Mathf.RoundToInt(position.z + z);

                    var pX = vX * noiseScale;
                    var pZ = vZ * noiseScale;
                    var y = Mathf.RoundToInt(Mathf.PerlinNoise(pX, pZ) * chunkHeight);

                    voxelData.Add(new VoxelData {
                        x = x,
                        y = y,
                        z = z,
                        value = 1
                    });
                }
            }
        }
    }
}