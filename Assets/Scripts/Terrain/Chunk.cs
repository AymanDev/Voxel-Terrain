using System;
using System.Collections.Generic;
using Terrain.Voxel;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Terrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Chunk : MonoBehaviour
    {
        [SerializeField] [ReadOnly] private ChunkRenderer chunkRenderer;
        [SerializeField] private MeshCollider meshCollider;

        private JobHandleExtended _jobHandle;

        /**
         * <summary>DO NOT WORK WITH THIS VARIABLE OUTSIDE JOB HANDLING</summary>
        */
        private NativeList<VoxelData> _voxels;

        [SerializeField] [ReadOnly] private ChunkData chunkData;

        public ChunkData ChunkData => chunkData;

        private bool _isVoxelsReady;

        public bool IsVoxelsReady => _isVoxelsReady;
        public bool IsChunkReady => _isVoxelsReady && chunkRenderer.IsRenderReady;

        public bool IsChunkActive => chunkRenderer.meshRenderer.enabled;

        public Vector3 chunkPosition;

        public void UpdateChunk(float noiseScale, int chunkSize, int chunkHeight)
        {
            _isVoxelsReady = false;

            chunkData = new ChunkData(chunkSize, chunkHeight);
            _voxels = new NativeList<VoxelData>(Allocator.TempJob);

            var localPosition = transform.localPosition;
            var job = new ChunkGenerateVoxelsJob()
            {
                voxelData = _voxels,
                noiseScale = noiseScale,
                chunkSize = chunkSize,
                chunkHeight = chunkHeight,
                position = localPosition
            };
            _jobHandle = new JobHandleExtended(job.Schedule());

            chunkPosition = localPosition;
        }

        private void Update()
        {
            if (IsChunkReady && !IsChunkActive)
            {
                ActivateChunk();
            }

            if (!_voxels.IsCreated)
            {
                return;
            }

            if (_jobHandle.Status == JobHandleStatus.AwaitingCompletion)
            {
                _jobHandle.Complete();

                chunkData.Voxels = new List<VoxelData>(_voxels.ToArray());
                _voxels.Dispose();
                _isVoxelsReady = true;

                GenerateMesh();
            }
        }

        public void DeactivateChunk()
        {
            chunkRenderer.meshRenderer.enabled = false;
            name = "Chunk (cached)";
            _isVoxelsReady = false;
            chunkRenderer.ResetRender();
        }

        private void ActivateChunk()
        {
            chunkRenderer.meshRenderer.enabled = true;
            name = "Chunk (active)";
        }

        private void GenerateMesh()
        {
            chunkRenderer.StartMeshTask(chunkData);
        }
    }
}
