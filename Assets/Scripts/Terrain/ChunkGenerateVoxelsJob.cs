using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Terrain {
    public struct VoxelPos {
        public Vector3 position;
        public int Id;
    }

    public struct ChunkGenerateVoxelsJob : IJobParallelFor {
        [WriteOnly] public NativeList<VoxelPos> VoxelPositions;

        [ReadOnly] public float NoiseScale;
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int ChunkHeight;
        [ReadOnly] public Vector3 Position;


        public void Execute(int index) {
            for (var x = 0; x < ChunkSize; x++) {
                for (var z = 0; z < ChunkSize; z++) {
                    var pX = (x + Position.x) * NoiseScale;
                    var pZ = (z + Position.z) * NoiseScale;
                    var y = Mathf.RoundToInt(Mathf.PerlinNoise(pX, pZ) * ChunkHeight);
                    VoxelPositions.Add(new VoxelPos {
                        position = new Vector3(Position.x + x, Position.y + y, Position.z + z),
                        Id = 1
                    });
                }
            }
        }
    }
}