using System;

[Serializable]
public class VoxelData {
    private readonly int[,,] _data;

    public int Width => _data.GetLength(0);
    public int Height => _data.GetLength(1);
    public int Depth => _data.GetLength(2);

    public VoxelData(int chunkSize, int chunkHeight) {
        _data = new int[chunkSize, chunkHeight, chunkSize];
    }

    public VoxelData(int[,,] data) {
        _data = data;
    }

    public void SetVoxel(int x, int y, int z, int voxel) {
        _data[x, y, z] = voxel;
    }

    public int GetVoxel(int x, int y, int z) {
        return _data[x, y, z];
    }

    public int[,,] GetVoxelChunk(int vX, int vY, int vZ, int chunkSize) {
        var voxels = new int[chunkSize, chunkSize, chunkSize];
        var x = 0;
        var y = 0;
        var z = 0;
        for (var dX = vX; dX < vX * chunkSize; dX++) {
            for (var dY = vY; dY < vY * chunkSize; dY++) {
                for (var dZ = vZ; dZ < vZ * chunkSize; dZ++) {
                    voxels[x, y, z] = _data[dX, dY, dZ];
                    z++;
                }

                y++;
            }

            x++;
        }

        return voxels;
    }

    public VoxelData GetVoxelDataChunk(int vX, int vY, int vZ, int chunkSize) {
        return new VoxelData(GetVoxelChunk(vX, vY, vZ, chunkSize));
    }

    public int GetNeighbor(int x, int y, int z, Direction dir) {
        var offsetToCheck = _offsets[(int) dir];
        var neighborCoord = new DataCoordinate(x + offsetToCheck.x, y + offsetToCheck.y, z + offsetToCheck.z);

        if (neighborCoord.x < 0 || neighborCoord.x >= Width ||
            neighborCoord.y < 0 || neighborCoord.y >= Height ||
            neighborCoord.z < 0 || neighborCoord.z >= Depth) {
            return 0;
        }

        return GetVoxel(neighborCoord.x, neighborCoord.y, neighborCoord.z);
    }

    private struct DataCoordinate {
        public int x;
        public int y;
        public int z;

        public DataCoordinate(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    // Order follows from Direction enum
    private readonly DataCoordinate[] _offsets = {
        new DataCoordinate(0, 0, 1),
        new DataCoordinate(1, 0, 0),
        new DataCoordinate(0, 0, -1),
        new DataCoordinate(-1, 0, 0),
        new DataCoordinate(0, 1, 0),
        new DataCoordinate(0, -1, 0),
    };
}