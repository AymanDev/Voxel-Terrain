using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour {
    [SerializeField] [ReadOnly] private VoxelRender voxelRender;
    [SerializeField] [ReadOnly] private VoxelData voxelData;


    public async UniTaskVoid GenerateChunk(float noiseScale, int chunkSize, int chunkHeight, Vector3 chunkPosition) {
        voxelData = await RequestVoxels(noiseScale, chunkSize, chunkHeight, chunkPosition);
        await UniTask.Run(() => voxelRender.GenerateVoxelMesh(voxelData));
        voxelRender.UpdateMesh();
    }

    private static UniTask<VoxelData> RequestVoxels(float noiseScale, int chunkSize, int chunkHeight,
        Vector3 chunkPosition) {
        return UniTask.Run(() => GenerateVoxels(noiseScale, chunkSize, chunkHeight, chunkPosition));
    }

    private static VoxelData GenerateVoxels(float noiseScale, int chunkSize, int chunkHeight, Vector3 position) {
        var data = new VoxelData(chunkSize, chunkHeight);
        for (var x = 0; x < chunkSize; x++) {
            for (var z = 0; z < chunkSize; z++) {
                var pX = (x + position.x) * noiseScale;
                var pZ = (z + position.z) * noiseScale;
                var y = Mathf.RoundToInt(Mathf.PerlinNoise(pX, pZ) * chunkHeight);
                data.SetVoxel(x, y, z, 1);
            }
        }

        return data;
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