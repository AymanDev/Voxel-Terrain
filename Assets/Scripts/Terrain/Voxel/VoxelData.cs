using System;

namespace Terrain.Voxel
{
    [Serializable]
    public struct VoxelData
    {
        private int? _x;
        private int? _y;
        private int? _z;
        private int? _value;
        public int X
        {
            get => _x ?? -1;
            set => _x = value;

        }
        public int Y
        {
            get => _y ?? -1;
            set => _y = value;
        }
        public int Z
        {
            get => _z ?? -1;
            set => _z = value;
        }
        public int Value
        {
            get => _value ?? -1;
            set => _value = value;
        }
    }
}
