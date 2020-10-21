using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
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

            public int MaximumChunksCount => unloadChunkDistance * unloadChunkDistance * 2;
        }

        [SerializeField] private GameObject chunkPrefab;

        [Header("Map generator settings")] [SerializeField] [Range(0, 1)]
        private float noiseScale = 0.05f;

        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private Transform playerTransform;

        [SerializeField] [ReadOnly]
        private SerializedDictionary<Vector3, Chunk> chunks = new SerializedDictionary<Vector3, Chunk>();

        [SerializeField] [ReadOnly] private Queue<Chunk> cachedChunks = new Queue<Chunk>();

        public int ActiveChunksCount => chunks.Count;
        public int CachedChunksCount => cachedChunks.Count;

        public Config config;

        public bool chunksAreLoading = false;
        private JobHandle _jobHandle;
        private NativeArray<Chunk.Data> _chunkDataArray;

        public bool jobsCompleted = false;


        private void Awake() {
            if (config.unloadChunkDistance <= config.loadChunkDistance) {
                throw new Exception("Unload chunk distance must be greater than load distance!");
            }

            PrepareChunkCache();
        }

        private void PrepareChunkCache() {
            for (var i = 0; i < config.MaximumChunksCount; i++) {
                SpawnCacheReadyChunk(transform.position);
            }
        }

        private void SpawnChunksAroundAndLoad(int originX, int originZ, int radius) {
            for (var x = -radius; x < radius; x++) {
                for (var z = -radius; z < radius; z++) {
                    var position = transform.localPosition +
                                   new Vector3((originX + x) * config.chunkSize, 0, (originZ + z) * config.chunkSize);
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
            // chunk.GenerateChunk(noiseScale, config.chunkSize, config.chunkHeight);
        }

        private void CacheChunk(Chunk chunk) {
            chunks.Remove(chunk.transform.localPosition);
            chunk.PrepareChunkForCaching();
            cachedChunks.Enqueue(chunk);
        }

        public Vector3 GetPlayerChunkPosition() {
            var playerChunkPosition = playerTransform.localPosition / config.chunkSize;
            playerChunkPosition.Set(Mathf.RoundToInt(playerChunkPosition.x), 0,
                Mathf.RoundToInt(playerChunkPosition.z));
            return playerChunkPosition;
        }

        public Chunk GetPlayerChunk() {
            return chunks[GetPlayerChunkPosition()];
        }


        public void StartJobLoadingChunk() {
            Debug.Log("Starting job");

            jobsCompleted = false;
            chunksAreLoading = true;


            var chunkList = cachedChunks.ToList();
            _chunkDataArray = new NativeArray<Chunk.Data>(chunkList.Count, Allocator.TempJob);
            for (var i = 0; i < chunkList.Count; i++) {
                var chunk = chunkList[i];
                _chunkDataArray[i] =
                    new Chunk.Data {
                        Position = chunk.transform.localPosition,
                        ChunkHeight = config.chunkHeight,
                        ChunkSize = config.chunkSize,
                        NoiseScale = noiseScale
                    };
            }

            var job = new ChunkGenerateVoxelsJob {
                ChunkDataArray = _chunkDataArray,
            };
            _jobHandle = job.Schedule(chunkList.Count, 1);
        }

        private void FinishJobLoadingChunk() {
            _chunkDataArray.Dispose();
            chunksAreLoading = false;
        }

        private void LateUpdate() {
            // _jobHandle.Complete();
        }

        private void FixedUpdate() {
            // var playerChunkPosition = GetPlayerChunkPosition();
            //
            // foreach (var chunk in new List<Chunk>(chunks.Values)) {
            //     if (!chunk) {
            //         continue;
            //     }
            //
            //     if (Vector3.Distance(chunk.transform.localPosition / chunkSize, playerChunkPosition) >=
            //         unloadChunkDistance) {
            //         CacheChunk(chunk);
            //     }
            // }
            //
            // var maximumChunksCount = unloadChunkDistance * unloadChunkDistance * 2;
            // if (chunks.Count < maximumChunksCount) {
            //     SpawnChunksAroundAndLoad((int) playerChunkPosition.x, (int) playerChunkPosition.z, loadChunkDistance);
            // }


            // if (!_chunksAreLoading && !_jobsCompleted) {
            //     StartJobLoadingChunk();
            // }

            if (chunksAreLoading && _jobHandle.IsCompleted && !jobsCompleted) {
                chunksAreLoading = false;
                jobsCompleted = true;
                _jobHandle.Complete();

                Debug.Log("Jobs are completed");
                foreach (var chunkData in _chunkDataArray) {
                    // foreach (var keyValue in chunkData.VoxelData) {
                    //     Debug.Log($"{keyValue.Key} = {keyValue.Value}");
                    // }

                    // Debug.Log(chunkData.data);
                }

                _chunkDataArray.Dispose();
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

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerTransform.position, config.loadChunkDistance * config.chunkSize);

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
}