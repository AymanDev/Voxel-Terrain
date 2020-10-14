using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Terrain : MonoBehaviour {
    [SerializeField] private GameObject chunkPrefab;

    [Header("Chunk settings")] [SerializeField]
    private int loadChunkDistance = 6;

    [Tooltip("Unload chunk distance MUST have be greater than load distance")] [SerializeField]
    private int unloadChunkDistance = 12;


    [SerializeField] private int chunkHeight = 512;
    [SerializeField] private int chunkSize = 16;

    [Header("Map generator settings")] [SerializeField] [Range(0, 1)]
    private float noiseScale = 0.05f;

    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private Transform playerTransform;

    [SerializeField] [ReadOnly]
    private SerializedDictionary<Vector3, Chunk> chunks = new SerializedDictionary<Vector3, Chunk>();

    [SerializeField] [ReadOnly] private Queue<Chunk> cachedChunks = new Queue<Chunk>();

    public int ActiveChunksCount => chunks.Count;
    public int MaximumChunksCount => unloadChunkDistance * unloadChunkDistance * 2;
    public int CachedChunksCount => cachedChunks.Count;

    private void Awake() {
        if (unloadChunkDistance <= loadChunkDistance) {
            throw new Exception("Unload chunk distance must be greater than load distance!");
        }

        PrepareChunkCache();
    }

    private void PrepareChunkCache() {
        for (var i = 0; i < MaximumChunksCount; i++) {
            SpawnCacheReadyChunk(transform.position);
        }
    }

    private void SpawnChunksAroundAndLoad(int originX, int originZ, int radius) {
        for (var x = -radius; x < radius; x++) {
            for (var z = -radius; z < radius; z++) {
                var position = transform.localPosition +
                               new Vector3((originX + x) * chunkSize, 0, (originZ + z) * chunkSize);
                if (chunks.ContainsKey(position)) {
                    continue;
                }

                var chunk = cachedChunks.Count > 0
                    ? SpawnCachedChunk(cachedChunks.Dequeue(), position)
                    : SpawnActiveChunk(position);
                LoadChunk(chunk);
            }
        }
    }

    private Chunk SpawnChunk(Vector3 position) {
        var chunkObj = Instantiate(chunkPrefab, transform);
        chunkObj.transform.localPosition = position;
        var chunk = chunkObj.GetComponent<Chunk>();
        return chunk;
    }

    private Chunk SpawnActiveChunk(Vector3 position) {
        var chunk = SpawnChunk(position);
        chunks.Add(position, chunk);
        return chunk;
    }

    private void SpawnCacheReadyChunk(Vector3 position) {
        var chunk = SpawnChunk(position);
        chunk.PrepareChunkForCaching();
        cachedChunks.Enqueue(chunk);
    }

    private Chunk SpawnCachedChunk(Chunk chunk, Vector3 position) {
        chunk.transform.localPosition = position;
        chunk.PrepareChunkForUnCaching();
        chunks.Add(position, chunk);
        return chunk;
    }

    private void LoadChunk(Chunk chunk) {
        chunk.GenerateChunk(noiseScale, chunkSize, chunkHeight);
    }

    private void CacheChunk(Chunk chunk) {
        chunks.Remove(chunk.transform.localPosition);
        chunk.PrepareChunkForCaching();
        cachedChunks.Enqueue(chunk);
    }

    public Vector3 GetPlayerChunkPosition() {
        var playerChunkPosition = playerTransform.localPosition / chunkSize;
        playerChunkPosition.Set(Mathf.RoundToInt(playerChunkPosition.x), 0, Mathf.RoundToInt(playerChunkPosition.z));
        return playerChunkPosition;
    }

    public Chunk GetPlayerChunk() {
        return chunks[GetPlayerChunkPosition()];
    }


    private void Update() {
        var playerChunkPosition = GetPlayerChunkPosition();

        foreach (var chunk in new List<Chunk>(chunks.Values)) {
            if (!chunk) {
                continue;
            }

            if (Vector3.Distance(chunk.transform.localPosition / chunkSize, playerChunkPosition) >=
                unloadChunkDistance) {
                CacheChunk(chunk);
            }
        }

        var maximumChunksCount = unloadChunkDistance * unloadChunkDistance * 2;
        if (chunks.Count < maximumChunksCount) {
            SpawnChunksAroundAndLoad((int) playerChunkPosition.x, (int) playerChunkPosition.z, loadChunkDistance);
        }
    }

    // private void CombineChunkMeshes() {
    //     meshFilter.mesh.Clear();
    //
    //     var meshFilters = GetComponentsInChildren<MeshFilter>(true)
    //         .Where(c => !c.transform.Equals(transform))
    //         .ToArray();
    //     var combine = new CombineInstance[meshFilters.Length];
    //
    //     for (var i = 0; i < meshFilters.Length; i++) {
    //         combine[i].mesh = meshFilters[i].sharedMesh;
    //         combine[i].transform = transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
    //         meshFilters[i].gameObject.SetActive(false);
    //     }
    //
    //     // meshFilter.mesh = new Mesh {indexFormat = UnityEngine.Rendering.IndexFormat.UInt32};
    //     meshFilter.mesh.CombineMeshes(combine);
    //     gameObject.SetActive(true);
    // }


    #region Gizmos

    private void DrawChunkBounds(Chunk chunk) {
        if (!chunk) {
            return;
        }

        var centeringPos = new Vector3(chunkSize / 2f, chunkHeight / 2f, chunkSize / 2f);
        var chunkCenter = chunk.transform.position + centeringPos;
        var chunkScale = new Vector3(chunkSize, chunkHeight, chunkSize);
        Gizmos.DrawWireCube(chunkCenter, chunkScale);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        // Gizmos.DrawSphere(transform.position, chunkDistance);
        Gizmos.DrawWireSphere(playerTransform.position, loadChunkDistance * chunkSize);

        Gizmos.color = Color.green;
        foreach (var chunk in chunks.Values) {
            DrawChunkBounds(chunk);
        }

        Gizmos.color = Color.red;
        foreach (var chunk in cachedChunks) {
            DrawChunkBounds(chunk);
        }
    }

    #endregion
}