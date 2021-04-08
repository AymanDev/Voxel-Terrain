using System;
using System.Collections.Generic;
using Terrain.Voxel;
using UnityEngine;

namespace Terrain
{
    [Serializable]
    public struct ChunkData
    {
        private int _maximumSize;

        public int Size { get; }

        public int Height { get; }

        public List<VoxelData> Voxels { get; set; }

        public ChunkData(int chunkSize, int chunkHeight)
        {
            Voxels = new List<VoxelData>();
            Size = chunkSize;
            Height = chunkHeight;
            _maximumSize = (Size * Size) * Height;
        }

        public int GetNeighbor(int x, int y, int z, Direction dir)
        {
            var offsetToCheck = VoxelUtils.Offsets[(int)dir];
            var neighborCoord =
                new VoxelUtils.DataCoordinate(x + offsetToCheck.x, y + offsetToCheck.y, z + offsetToCheck.z);

            if (neighborCoord.x < 0 || neighborCoord.x >= Size ||
                neighborCoord.y < 0 || neighborCoord.y >= Height ||
                neighborCoord.z < 0 || neighborCoord.z >= Size)
            {
                return 0;
            }

            var voxelData = Voxels.Find(data =>
                data.X == neighborCoord.x && data.Y == neighborCoord.y && data.Z == neighborCoord.z);
            return voxelData.Value;
        }

        public static int GetNeighbor(List<VoxelData> voxels, int x, int y, int z, Direction dir)
        {
            var offsetToCheck = VoxelUtils.Offsets[(int)dir];
            var neighborCoord =
                new VoxelUtils.DataCoordinate(x + offsetToCheck.x, y + offsetToCheck.y, z + offsetToCheck.z);

            if (neighborCoord.x < 0 || neighborCoord.x >= TerrainManager.instance.config.chunkSize ||
                neighborCoord.y < 0 || neighborCoord.y >= TerrainManager.instance.config.chunkHeight ||
                neighborCoord.z < 0 || neighborCoord.z >= TerrainManager.instance.config.chunkSize)
            {
                return 0;
            }

            var voxelData = voxels.Find(data =>
                data.X == neighborCoord.x && data.Y == neighborCoord.y && data.Z == neighborCoord.z);
            return voxelData.Value;
        }

        public VoxelData GetVoxelAtPosition(Vector3 position)
        {
            var voxel = Voxels.Find(v => v.X == (int)position.x && v.Y == (int)position.y && v.Z == (int)position.z);
            return voxel;
        }
    }
}
