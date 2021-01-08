using Unity.Entities;

namespace Terrain.ECS {
    public struct VoxelSpawner : IComponentData {
        public Entity prefab;
        public float maxDistanceFromSpawner;
        public int maxVoxels;
        public int spawnedVoxelsCount;
    }
}