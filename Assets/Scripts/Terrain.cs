using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Terrain : MonoBehaviour {
    [SerializeField] private GameObject chunkPrefab;

    [Header("Chunk settings")] [SerializeField]
    private int loadChunkDistance = 6;

    [Tooltip("Unload chunk distance MUST have be greater than load distance")] [SerializeField]
    private int unloadChunkDistance = 3;

    [SerializeField] private int chunkHeight = 512;
    [SerializeField] private int chunkSize = 16;

    [Header("Map generator settings")] [SerializeField] [Range(0, 1)]
    private float noiseScale = 0.05f;

    [SerializeField] [Range(0, 1)] private float threshold = 0.5f;
    [SerializeField] [ReadOnly] private MeshFilter meshFilter;
    [SerializeField] private bool areCombiningMeshes = false;
    [SerializeField] private Transform playerTransform;

    [SerializeField] [ReadOnly]
    private SerializedDictionary<Vector3, Chunk> chunks = new SerializedDictionary<Vector3, Chunk>();

    private int _activeChunkX;
    private int _activeChunkZ;

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();

        if (unloadChunkDistance <= loadChunkDistance) {
            throw new Exception("Unload chunk distance must be greater than load distance!");
        }
    }

    public void Start() {
        var position = transform.position;
        _activeChunkX = (int) position.x;
        _activeChunkZ = (int) position.z;
        LoadChunksAround(_activeChunkX, _activeChunkZ, loadChunkDistance);
    }

    private void LoadChunksAround(int originX, int originZ, int radius) {
        for (var x = -radius; x < radius; x++) {
            for (var z = -radius; z < radius; z++) {
                LoadChunk(originX + x, originZ + z);
            }
        }
    }

    private void LoadChunk(int x, int z) {
        var position = transform.localPosition + new Vector3(x * chunkSize, 0, z * chunkSize);
        if (chunks.ContainsKey(position)) {
            return;
        }

        var chunkObj = Instantiate(chunkPrefab, transform);
        chunkObj.transform.localPosition = position;

        var chunk = chunkObj.GetComponent<Chunk>();
        chunks.Add(position, chunk);
        UniTask.RunOnThreadPool(() => chunk.GenerateChunk(noiseScale, chunkSize, chunkHeight, position).Forget());
    }

    private void Update() {
        var playerChunkPosition = playerTransform.localPosition / chunkSize;
        playerChunkPosition.Set(Mathf.RoundToInt(playerChunkPosition.x), 0, Mathf.RoundToInt(playerChunkPosition.z));

        foreach (var chunk in new List<Chunk>(chunks.Values).Where(chunk =>
            Vector3.Distance(chunk.transform.localPosition / chunkSize, playerChunkPosition) >
            unloadChunkDistance)) {
            Destroy(chunk.gameObject);
            chunks.Remove(chunk.transform.localPosition);
        }

        var maximumChunksCount = unloadChunkDistance * unloadChunkDistance * 2;
        if (chunks.Count < maximumChunksCount - 5) {
            LoadChunksAround((int) playerChunkPosition.x, (int) playerChunkPosition.z, loadChunkDistance);
        }
    }

    private void CombineChunkMeshes() {
        meshFilter.mesh.Clear();

        var meshFilters = GetComponentsInChildren<MeshFilter>(true)
            .Where(c => !c.transform.Equals(transform))
            .ToArray();
        var combine = new CombineInstance[meshFilters.Length];

        for (var i = 0; i < meshFilters.Length; i++) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }

        meshFilter.mesh = new Mesh {indexFormat = UnityEngine.Rendering.IndexFormat.UInt32};
        meshFilter.mesh.CombineMeshes(combine);
        gameObject.SetActive(true);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        // Gizmos.DrawSphere(transform.position, chunkDistance);
        Gizmos.DrawWireSphere(playerTransform.position, loadChunkDistance * chunkSize);

        Gizmos.color = Color.red;
        foreach (var chunk in chunks.Values) {
            var centeringPos = new Vector3(chunkSize / 2f, chunkHeight / 2f, chunkSize / 2f);
            var chunkCenter = chunk.transform.position + centeringPos;
            var chunkScale = new Vector3(chunkSize, chunkHeight, chunkSize);
            Gizmos.DrawWireCube(chunkCenter, chunkScale);
        }
    }
}