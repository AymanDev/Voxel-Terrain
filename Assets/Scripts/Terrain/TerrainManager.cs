using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Terrain {
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TerrainManager : MonoBehaviour {
        [Serializable]
        public struct Config {
            public int loadChunkDistance;
            public int unloadChunkDistance;
            public int chunkSize;
            public int chunkHeight;

            public int maximumParallelChunksLoading;

            public int maximumChunksModifier;
            public int MaximumChunksCount => unloadChunkDistance * unloadChunkDistance * maximumChunksModifier;
        }

        [SerializeField] private GameObject chunkPrefab;

        [Header("Map generator settings")] [SerializeField] [UnityEngine.Range(0, 1)]
        private float noiseScale = 0.05f;

        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private Transform playerTransform;

        [SerializeField] [ReadOnly] private Queue<Chunk> cachedChunks = new Queue<Chunk>();
        [SerializeField] [ReadOnly] private List<Chunk> readyToLoadChunks = new List<Chunk>();
        [SerializeField] [ReadOnly] private List<Chunk> loadingChunks = new List<Chunk>();
        [SerializeField] [ReadOnly] private List<Chunk> loadedChunks = new List<Chunk>();

        public int ActiveChunksCount => loadedChunks.Count;
        public int CachedChunksCount => cachedChunks.Count;
        public int ReadyToLoadChunksCount => readyToLoadChunks.Count;
        public int LoadingChunksCount => loadingChunks.Count;

        public Config config;

        public static TerrainManager instance;

        public bool showChunksBoundingBox;

        private void Awake() {
            instance = this;

            ResetTerrain();
            if (config.unloadChunkDistance <= config.loadChunkDistance) {
                throw new Exception("Unload chunk distance must be greater than load distance!");
            }

            PrepareChunkCache();

            StartCoroutine(ChunksUpdate());
        }


        private void PrepareChunkCache() {
            for (var i = 0; i < config.MaximumChunksCount; i++) {
                SpawnCacheReadyChunk(transform.position);
            }
        }

        private IEnumerator ChunksUpdate() {
            while (true) {
                TryToCacheChunksPool(loadedChunks);

                if (readyToLoadChunks.Count > 0) {
                    TryToCacheChunksPool(readyToLoadChunks);
                }

                var playerChunkPosition = GetPlayerChunkPosition();
                var maximumChunksCount = config.MaximumChunksCount;
                if (loadedChunks.Count < maximumChunksCount) {
                    SpawnChunksAround(playerChunkPosition, config.loadChunkDistance);
                }


                yield return new WaitForSecondsRealtime(1f);
            }
        }

        private void SpawnChunksAround(Vector3 origin, int radius) {
            var playerPos = GetPlayerChunkPosition();
            for (var x = -radius; x < radius; x++) {
                for (var z = -radius; z < radius; z++) {
                    var position = transform.localPosition +
                                   new Vector3((origin.x + x), 0, (origin.z + z));
                    if (Vector3.Distance(playerPos, position) > config.loadChunkDistance) {
                        continue;
                    }


                    position *= config.chunkSize;

                    var loadedChunk = GetChunkInPosition(loadedChunks, position);
                    var loadingChunk = GetChunkInPosition(loadingChunks, position);
                    var readyToLoadChunk = GetChunkInPosition(readyToLoadChunks, position);
                    if (loadedChunk || readyToLoadChunk || loadingChunk || cachedChunks.Count == 0) {
                        continue;
                    }

                    var chunk = SpawnCachedChunk(cachedChunks.Dequeue(), position);
                    readyToLoadChunks.Add(chunk);
                }
            }
        }

        private Chunk InstantiateChunk(Vector3 position) {
            var chunkObj = Instantiate(chunkPrefab, transform);
            chunkObj.transform.localPosition = position;
            var chunk = chunkObj.GetComponent<Chunk>();
            return chunk;
        }

        private void SpawnCacheReadyChunk(Vector3 position) {
            var chunk = InstantiateChunk(position);
            chunk.DeactivateChunk();
            cachedChunks.Enqueue(chunk);
        }

        private Chunk SpawnCachedChunk(Chunk chunk, Vector3 position) {
            chunk.DeactivateChunk();
            chunk.transform.localPosition = position;
            return chunk;
        }

        private void LoadChunk(Chunk chunk) {
            loadedChunks.Add(chunk);
            chunk.UpdateChunk(noiseScale, config.chunkSize, config.chunkHeight);
        }

        private void CacheChunk(Chunk chunk) {
            if (loadingChunks.Contains(chunk)) {
                return;
            }

            if (readyToLoadChunks.Contains(chunk)) {
                readyToLoadChunks.Remove(chunk);
            }

            loadedChunks.Remove(chunk);
            chunk.DeactivateChunk();
            cachedChunks.Enqueue(chunk);
        }

        public Vector3 GetPlayerChunkPosition() {
            return GetChunkPosition(playerTransform.position);
        }

        public Vector3 GetChunkPosition(Vector3 position) {
            var pos = position / config.chunkSize;
            pos.Set(Mathf.RoundToInt(pos.x), 0,
                Mathf.RoundToInt(pos.z));
            return pos;
        }

        public static Chunk GetChunkInPosition(List<Chunk> chunks, Vector3 position) {
            return chunks.Find(c => c.transform.position.Equals(position));
        }

        private void ResetTerrain() {
            foreach (var chunk in loadedChunks) {
                Destroy(chunk.gameObject);
            }

            foreach (var cachedChunk in cachedChunks.Where(cachedChunk => cachedChunk)) {
                Destroy(cachedChunk);
            }

            cachedChunks.Clear();
            loadedChunks.Clear();
        }

        private void TryToCacheChunksPool(IEnumerable<Chunk> chunks) {
            var playerChunkPosition = GetPlayerChunkPosition();

            foreach (var chunk in new List<Chunk>(chunks)) {
                var chunkPos = GetChunkPosition(chunk.transform.localPosition);
                var distance = Mathf.RoundToInt(
                    Vector3.Distance(chunkPos, playerChunkPosition));

                if (distance > config.unloadChunkDistance) {
                    CacheChunk(chunk);
                }
            }
        }

        private void FixedUpdate() {
            LoadChunks();
        }

        private void LoadChunks() {
            loadingChunks = loadingChunks.Where(chunk => !chunk.IsChunkReady).ToList();

            if (readyToLoadChunks.Count == 0) {
                return;
            }

            while (loadingChunks.Count <= config.maximumParallelChunksLoading && readyToLoadChunks.Count > 0) {
                var chunk = readyToLoadChunks.First();
                loadingChunks.Add(chunk);
                LoadChunk(chunk);
                readyToLoadChunks.Remove(chunk);
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

            var centeringPos = new Vector3(config.chunkSize / 2f, config.chunkHeight / 2f, config.chunkSize / 2f);
            var chunkCenter = chunk.transform.position + centeringPos;
            var chunkScale = new Vector3(config.chunkSize, config.chunkHeight, config.chunkSize);
            Gizmos.DrawWireCube(chunkCenter, chunkScale);
        }

        private void OnDrawGizmos() {
            if (!showChunksBoundingBox) {
                return;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerTransform.position, config.loadChunkDistance * config.chunkSize);

            Gizmos.color = Color.green;
            foreach (var chunk in loadedChunks) {
                DrawChunkBounds(chunk);
            }

            Gizmos.color = Color.yellow;
            foreach (var chunk in loadingChunks) {
                DrawChunkBounds(chunk);
            }

            Gizmos.color = Color.magenta;
            foreach (var chunk in readyToLoadChunks) {
                DrawChunkBounds(chunk);
            }

            Gizmos.color = Color.red;
            foreach (var chunk in cachedChunks) {
                DrawChunkBounds(chunk);
            }
        }

        #endregion
    }
}