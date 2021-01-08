using Unity.Entities;

namespace Terrain.ECS {
    public struct VoxelStatus : ISharedComponentData {
        public Status status;

        public enum Status {
            Spawned,
            Generated,
            Rendered,
        }
    }
}