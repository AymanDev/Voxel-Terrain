using System;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour {
    [SerializeField] [ReadOnly] private VoxelRender voxelRender;
    [SerializeField] private VoxelData voxelData;
    [SerializeField] private MeshCollider meshCollider;

    public void PrepareChunkForCaching() {
        voxelRender.meshRenderer.enabled = false;
        // meshCollider.isTrigger = true;
    }

    public void PrepareChunkForUnCaching() {
        voxelRender.meshRenderer.enabled = true;
        // meshCollider.isTrigger = false;
    }

    public void GenerateChunk(float noiseScale, int chunkSize, int chunkHeight) {
        var pos = transform.localPosition;
        UniTask.Run(() => TryGenerateVoxels(noiseScale, chunkSize, chunkHeight, pos)).Forget();
    }

    private async UniTaskVoid TryGenerateVoxels(float noiseScale, int chunkSize, int chunkHeight, Vector3 position) {
        voxelData = await UniTask.RunOnThreadPool(() => GenerateVoxels(noiseScale, chunkSize, chunkHeight, position));

        UniTask.Run(() => TryGenerateMesh(voxelData)).Forget();
    }

    private async UniTaskVoid TryGenerateMesh(VoxelData data) {
        var render = voxelRender;
        var (verts, tris) = await UniTask.RunOnThreadPool(() => render.GenerateVoxelMesh(data));
        render.UpdateMesh(verts, tris);
        meshCollider.sharedMesh = render.Mesh;
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