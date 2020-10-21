using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour {
    [SerializeField] [ReadOnly] private VoxelRender voxelRender;
    [SerializeField] private MeshCollider meshCollider;

    public struct VoxelPosition : IEquatable<VoxelPosition> {
        public int X;
        public int Y;
        public int Z;

        public override string ToString() {
            return $"X: {X} Y: {Y} Z: {Z}";
        }

        public bool Equals(VoxelPosition other) {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj) {
            return obj is VoxelPosition other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
                return hashCode;
            }
        }
    }

    [Serializable]
    public struct VoxelData {
        public int x;
        public int y;
        public int z;
        public int value;
    }


    [Serializable]
    public struct Data {
        // [WriteOnly] public NativeHashMap<VoxelPosition, int> VoxelData;
        public NativeArray<VoxelData> data;
        [ReadOnly] public float NoiseScale;
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int ChunkHeight;
        [ReadOnly] public Vector3 Position;


        public void UpdateVoxelData() {
            data = new NativeArray<VoxelData>(0, Allocator.TempJob);
            // VoxelData = new NativeHashMap<VoxelPosition, int>();
            // for (var x = 0; x < ChunkSize; x++) {
            //     for (var z = 0; z < ChunkSize; z++) {
            //         var pX = (x + Position.x) * NoiseScale;
            //         var pZ = (z + Position.z) * NoiseScale;
            //         var y = Mathf.RoundToInt(Mathf.PerlinNoise(pX, pZ) * ChunkHeight);
            //
            //         var key = new VoxelPosition {
            //             X = x,
            //             Y = y,
            //             Z = z
            //         };
            //         VoxelData.Add(key, 1);
            //     }
            // }

            // int[,,] d = GenerateVoxels(_noiseScale, _chunkSize, _chunkHeight, _position);
        }
    }

    public void PrepareChunkForCaching() {
        voxelRender.meshRenderer.enabled = false;
    }

    public void PrepareChunkForUnCaching() {
        voxelRender.meshRenderer.enabled = true;
    }


    private async UniTaskVoid TryGenerateMesh(ChunkData data) {
        var render = voxelRender;
        var (verts, tris) = await UniTask.RunOnThreadPool(() => render.GenerateVoxelMesh(data));
        render.UpdateMesh(verts, tris);
        meshCollider.sharedMesh = render.Mesh;
    }

    private static int[,,] GenerateVoxels(float noiseScale, int chunkSize, int chunkHeight, Vector3 position) {
        var voxels = new int[chunkSize, chunkHeight, chunkSize];
        for (var x = 0; x < chunkSize; x++) {
            for (var z = 0; z < chunkSize; z++) {
                var pX = (x + position.x) * noiseScale;
                var pZ = (z + position.z) * noiseScale;
                var y = Mathf.RoundToInt(Mathf.PerlinNoise(pX, pZ) * chunkHeight);
                voxels[x, y, z] = 1;
            }
        }

        return voxels;
    }


    // Not working properly. Have to fix this
    public static float Perlin3D(float x, float y, float z) {
        var ab = Mathf.PerlinNoise(x, y);
        var bc = Mathf.PerlinNoise(y, z);
        var ac = Mathf.PerlinNoise(x, z);

        var ba = Mathf.PerlinNoise(y, x);
        var cb = Mathf.PerlinNoise(z, y);
        var ca = Mathf.PerlinNoise(z, x);

        var abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
    }
}