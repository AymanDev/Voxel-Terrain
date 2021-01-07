using System.Collections.Generic;
using Terrain.ECS;
using Terrain.Voxel;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Terrain {
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Chunk : MonoBehaviour {
        [SerializeField] [ReadOnly] private VoxelRender voxelRender;
        [SerializeField] private MeshCollider meshCollider;

        private JobHandleExtended _jobHandle;

        /**
         * <summary>DO NOT WORK WITH THIS VARIABLE OUTSIDE JOB HANDLING</summary>
        */
        private NativeList<VoxelData> _voxels;

        [SerializeField] [ReadOnly] private ChunkData chunkData;

        private bool _isVoxelsReady;

        public bool IsChunkReady => _isVoxelsReady && voxelRender.IsRenderReady;

        public bool IsChunkActive => voxelRender.meshRenderer.enabled;

        public void UpdateChunk(float noiseScale, int chunkSize, int chunkHeight) {
            _isVoxelsReady = false;

            chunkData = new ChunkData(chunkSize, chunkHeight);
            _voxels = new NativeList<VoxelData>(Allocator.TempJob);

            var job = new ChunkGenerateVoxelsJob() {
                voxelData = _voxels,
                noiseScale = noiseScale,
                chunkSize = chunkSize,
                chunkHeight = chunkHeight,
                position = transform.localPosition
            };
            _jobHandle = new JobHandleExtended(job.Schedule());
        }

        private void Update() {
            if (IsChunkReady && !IsChunkActive) {
                ActivateChunk();
            }

            if (!_voxels.IsCreated) {
                return;
            }

            if (_jobHandle.Status == JobHandleStatus.AwaitingCompletion) {
                _jobHandle.Complete();

                chunkData.Voxels = new List<VoxelData>(_voxels.ToArray());
                _voxels.Dispose();
                _isVoxelsReady = true;

                GenerateMesh();
            }
        }

        public void DeactivateChunk() {
            voxelRender.meshRenderer.enabled = false;
            name = "Chunk (cached)";
            _isVoxelsReady = false;
            voxelRender.ResetRender();
        }

        private void ActivateChunk() {
            voxelRender.meshRenderer.enabled = true;
            name = "Chunk (active)";
        }

        private void GenerateMesh() {
            voxelRender.GenerateMesh(chunkData);
        }
    }
}