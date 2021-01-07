using Unity.Entities;

namespace Terrain.ECS {
    public struct ChunkSpawner : IComponentData {
        public Entity prefab;
        public float maxDistanceFromSpawner;
        public float secondsBetweenSpawns;
        public float secondsToNextSpawn;
    }
}