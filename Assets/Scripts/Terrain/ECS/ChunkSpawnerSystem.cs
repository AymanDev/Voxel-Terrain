using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Terrain.ECS {
    public class ChunkSpawnerSystem : SystemBase {
        private BeginSimulationEntityCommandBufferSystem _beginSimulationSystem;
        
        protected override void OnCreate() {
            base.OnCreate();
            _beginSimulationSystem =
                World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var deltaTime = Time.DeltaTime;
            var ecb = _beginSimulationSystem.CreateCommandBuffer().AsParallelWriter();
            var random = new Random((uint) UnityEngine.Random.Range(0, int.MaxValue));

            Entities.ForEach((int entityInQueryIndex, ref ChunkSpawner spawner, ref Translation translation,
                in LocalToWorld localToWorld) => {
                spawner.secondsToNextSpawn -= deltaTime;
                if (spawner.secondsToNextSpawn >= 0) {
                    return;
                }

                spawner.secondsToNextSpawn += spawner.secondsBetweenSpawns;

                var instance = ecb.Instantiate(entityInQueryIndex, spawner.prefab);
                ecb.SetComponent(entityInQueryIndex, instance, new Translation() {
                    Value = localToWorld.Position + random.NextFloat3Direction() * random.NextFloat() *
                        spawner.maxDistanceFromSpawner
                });
            }).ScheduleParallel();

            _beginSimulationSystem.AddJobHandleForProducer(Dependency);
        }
    }
}