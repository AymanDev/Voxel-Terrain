using System;

namespace Terrain.Voxel {
    [Serializable]
    public struct VoxelData {
        public int x;
        public int y;
        public int z;
        public int value;
    }
}