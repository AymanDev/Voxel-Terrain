using System;
using System.Collections.Generic;
using System.Linq;
using Terrain.Mesh;
using Terrain.Voxel;
using Unity.Collections;
using UnityEngine;

namespace Terrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TerrainManager : MonoBehaviour
    {
        [Serializable]
        public struct Config
        {
            public int chunkSize;
            public int chunkHeight;

            public int parallelLoadingPoolSize;
            public int chunksCachePoolSize;
        }

        [SerializeField] private GameObject chunkPrefab;

        [Header("Map generator settings")] [SerializeField] [Range(0, 1)]
        private float noiseScale = 0.05f;


        public Config config;

        [SerializeField] [ReadOnly] private Queue<Chunk> chunkCachePool = new Queue<Chunk>();
        [SerializeField] [ReadOnly] private List<Chunk> readyToLoadChunks = new List<Chunk>();
        [SerializeField] [ReadOnly] private List<Chunk> loadingChunks = new List<Chunk>();
        [SerializeField] [ReadOnly] private List<Chunk> loadedChunks = new List<Chunk>();

        public List<Chunk> LoadedChunks => loadedChunks;

        public static TerrainManager instance;

        private void Awake()
        {
            instance = this;

            ResetTerrain();
            PrepareChunkCachePool();
        }


        private void PrepareChunkCachePool()
        {
            for (var i = 0; i < config.chunksCachePoolSize; i++)
            {
                SpawnChunkForCachePool(transform.localPosition);
            }
        }

        public void SpawnChunksAround(Vector3 origin, int radius)
        {
            if (chunkCachePool.Count == 0)
            {
                return;
            }

            var gridOriginPosition = GetInGridPosition(origin);
            for (var x = -radius; x < radius; x++)
            {
                for (var z = -radius; z < radius; z++)
                {
                    var checkingPosition = new Vector3(
                        Mathf.RoundToInt(gridOriginPosition.x + x),
                        0,
                        Mathf.RoundToInt(gridOriginPosition.z + z)) * (config.chunkSize - 1);
                    var loadedChunk = GetChunkInPosition(loadedChunks, checkingPosition);
                    var loadingChunk = GetChunkInPosition(loadingChunks, checkingPosition);
                    var readyToLoadChunk = GetChunkInPosition(readyToLoadChunks, checkingPosition);

                    if (loadedChunk || readyToLoadChunk || loadingChunk || chunkCachePool.Count == 0)
                    {
                        continue;
                    }

                    var chunk = SpawnCachedChunk(chunkCachePool.Dequeue(), checkingPosition);
                    readyToLoadChunks.Add(chunk);
                }
            }
        }


        private Chunk InstantiateChunk(Vector3 position)
        {
            var chunkObj = Instantiate(chunkPrefab, transform);
            chunkObj.transform.localPosition = position;
            var chunk = chunkObj.GetComponent<Chunk>();
            return chunk;
        }

        private void SpawnChunkForCachePool(Vector3 position)
        {
            var chunk = InstantiateChunk(position);
            chunk.DeactivateChunk();
            chunkCachePool.Enqueue(chunk);
        }

        private Chunk SpawnCachedChunk(Chunk chunk, Vector3 position)
        {
            chunk.DeactivateChunk();
            chunk.transform.localPosition = position;
            return chunk;
        }

        private void LoadChunk(Chunk chunk)
        {
            loadedChunks.Add(chunk);
            chunk.UpdateChunk(noiseScale, config.chunkSize, config.chunkSize);
        }

        public void MoveChunkToCachePool(Chunk chunk)
        {
            if (loadingChunks.Contains(chunk))
            {
                return;
            }

            if (readyToLoadChunks.Contains(chunk))
            {
                readyToLoadChunks.Remove(chunk);
            }

            loadedChunks.Remove(chunk);
            chunk.DeactivateChunk();
            chunkCachePool.Enqueue(chunk);
        }

        public Vector3 GetInGridPosition(Vector3 position)
        {
            var pos = position / config.chunkSize;
            pos.Set(Mathf.CeilToInt(pos.x), 0,
                Mathf.CeilToInt(pos.z));
            return pos;
        }

        public static Chunk GetChunkInPosition(List<Chunk> chunks, Vector3 position)
        {
            return chunks.Find(c => c.chunkPosition.Equals(position));
        }

        private void ResetTerrain()
        {
            foreach (var chunk in loadedChunks)
            {
                Destroy(chunk.gameObject);
            }

            foreach (var cachedChunk in chunkCachePool.Where(cachedChunk => cachedChunk))
            {
                Destroy(cachedChunk);
            }

            chunkCachePool.Clear();
            loadedChunks.Clear();
        }

        // private void TryToCacheChunksPool(IEnumerable<Chunk> chunks) {
        // var playerChunkPosition = GetPlayerChunkPosition();

        // foreach (var chunk in new List<Chunk>(chunks)) {
        // var chunkPos = GetChunkPosition(chunk.transform.localPosition);
        // var distance = Mathf.RoundToInt(
        // Vector3.Distance(chunkPos, playerChunkPosition));

        // if (distance > config.unloadChunkDistance) {
        // CacheChunk(chunk);
        // }
        // }
        // }

        private void FixedUpdate()
        {
            LoadChunks();
        }

        private void LoadChunks()
        {
            if (readyToLoadChunks.Count == 0)
            {
                return;
            }

            loadingChunks = loadingChunks.Where(chunk => !chunk.IsChunkReady).ToList();
            if (loadingChunks.Count > config.parallelLoadingPoolSize)
            {
                return;
            }

            while (loadingChunks.Count <= config.parallelLoadingPoolSize && readyToLoadChunks.Count > 0)
            {
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

        public VoxelData GetVoxelDataInPosition(Vector3 chunkPosition, Vector3 voxelPosition)
        {
            var loadedChunk = GetChunkInPosition(loadedChunks, chunkPosition);
            var loadingChunk = GetChunkInPosition(loadingChunks, chunkPosition);

            var chunk = loadingChunk.IsVoxelsReady ? loadingChunk : loadedChunk;

            return chunk.ChunkData.GetVoxelAtPosition(voxelPosition);
        }

        #region Gizmos

        private void DrawChunkBounds(Chunk chunk)
        {
            if (!chunk)
            {
                return;
            }

            var centeringPos = new Vector3(config.chunkSize / 2f, config.chunkHeight / 2f, config.chunkSize / 2f);
            var chunkCenter = chunk.transform.position + centeringPos;
            var chunkScale = new Vector3(config.chunkSize, config.chunkHeight, config.chunkSize);
            Gizmos.DrawWireCube(chunkCenter, chunkScale);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            foreach (var chunk in loadedChunks)
            {
                DrawChunkBounds(chunk);
            }

            Gizmos.color = Color.yellow;
            foreach (var chunk in loadingChunks)
            {
                DrawChunkBounds(chunk);
            }

            Gizmos.color = Color.magenta;
            foreach (var chunk in readyToLoadChunks)
            {
                DrawChunkBounds(chunk);
            }

            Gizmos.color = Color.red;
            foreach (var chunk in chunkCachePool)
            {
                DrawChunkBounds(chunk);
            }
        }

        #endregion
    }
}
