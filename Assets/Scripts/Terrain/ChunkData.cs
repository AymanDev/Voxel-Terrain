using System;
using System.Collections.Generic;
using Terrain.Voxel;

namespace Terrain {
    [Serializable]
    public struct ChunkData {
        private int _maximumSize;

        public int Size { get; }

        public int Height { get; }

        public List<VoxelData> Voxels { get; set; }

        public ChunkData(int chunkSize, int chunkHeight) {
            Voxels = new List<VoxelData>();
            Size = chunkSize;
            Height = chunkHeight;
            _maximumSize = (Size * Size) * Height;
        }

        // public void SetVoxel(int x, int y, int z, int voxel) {
        // _data[x, y, z] = voxel;
        // }

        // public int GetVoxel(int x, int y, int z) {
        // return _data[x, y, z];
        // }

        // public int[,,] GetVoxelChunk(int vX, int vY, int vZ, int chunkSize) {
        //     var voxels = new int[chunkSize, chunkSize, chunkSize];
        //     var x = 0;
        //     var y = 0;
        //     var z = 0;
        //     for (var dX = vX; dX < vX * chunkSize; dX++) {
        //         for (var dY = vY; dY < vY * chunkSize; dY++) {
        //             for (var dZ = vZ; dZ < vZ * chunkSize; dZ++) {
        //                 voxels[x, y, z] = _data[dX, dY, dZ];
        //                 z++;
        //             }
        //
        //             y++;
        //         }
        //
        //         x++;
        //     }
        //
        //     return voxels;
        // }
        //
        // public ChunkData GetVoxelDataChunk(int vX, int vY, int vZ, int chunkSize) {
        //     return new ChunkData(GetVoxelChunk(vX, vY, vZ, chunkSize));
        // }

        public int GetNeighbor(int x, int y, int z, Direction dir) {
            var offsetToCheck = VoxelUtils.Offsets[(int) dir];
            var neighborCoord =
                new VoxelUtils.DataCoordinate(x + offsetToCheck.x, y + offsetToCheck.y, z + offsetToCheck.z);

            if (neighborCoord.x < 0 || neighborCoord.x >= Size ||
                neighborCoord.y < 0 || neighborCoord.y >= Height ||
                neighborCoord.z < 0 || neighborCoord.z >= Size) {
                return 0;
            }

            var voxelData = Voxels.Find(data =>
                data.x == neighborCoord.x && data.y == neighborCoord.y && data.z == neighborCoord.z);
            return voxelData.value;
        }

        public static int GetNeighbor(List<VoxelData> voxels, int x, int y, int z, Direction dir) {
            var offsetToCheck = VoxelUtils.Offsets[(int) dir];
            var neighborCoord =
                new VoxelUtils.DataCoordinate(x + offsetToCheck.x, y + offsetToCheck.y, z + offsetToCheck.z);

            if (neighborCoord.x < 0 || neighborCoord.x >= TerrainManager.instance.config.chunkSize ||
                neighborCoord.y < 0 || neighborCoord.y >= TerrainManager.instance.config.chunkHeight ||
                neighborCoord.z < 0 || neighborCoord.z >= TerrainManager.instance.config.chunkSize) {
                return 0;
            }

            var voxelData = voxels.Find(data =>
                data.x == neighborCoord.x && data.y == neighborCoord.y && data.z == neighborCoord.z);
            return voxelData.value;
        }


        // Order follows from Direction enum
    }
}