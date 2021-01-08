using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Terrain.ECS {
    public class VoxelSpawnerSystem : SystemBase {
        private BeginSimulationEntityCommandBufferSystem _beginSimulationSystem;
        private EntityQuery _voxelQuery;

        protected override void OnCreate() {
            base.OnCreate();
            _beginSimulationSystem =
                    World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
            _voxelQuery = GetEntityQuery(
                    ComponentType.ReadOnly<Voxel>(),
                    ComponentType.ReadOnly<Translation>()
            );
        }

        protected override void OnUpdate() {
            
            
                        
            var ecb = _beginSimulationSystem.CreateCommandBuffer().AsParallelWriter();
            var positions = _voxelQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

            Entities
                    .WithReadOnly(positions)
                    .WithDisposeOnCompletion(positions)
                    .WithoutBurst()
                    // .WithDisposeOnCompletion(typeChunks)
                    .ForEach((
                            Entity spawnerEntity,
                            int entityInQueryIndex,
                            ref VoxelSpawner spawner,
                            in LocalToWorld localToWorld
                    ) => {
                        if (spawner.spawnedVoxelsCount >= spawner.maxVoxels) {
                            return;
                        }
                        // var positionType = GetComponentTypeHandle<Translation>();

                        for (var x = -spawner.maxDistanceFromSpawner; x < spawner.maxDistanceFromSpawner; x++) {
                            for (var z = -spawner.maxDistanceFromSpawner; z < spawner.maxDistanceFromSpawner; z++) {
                                var position = new int3(localToWorld.Position +
                                                        new float3(x, 0, z));
                                if (positions.Any(p => p.Value.Equals(position))) {
                                    continue;
                                }

                                position.y = GetYForVoxel(position.x, position.z, 0.01f);

                                var instance = ecb.Instantiate(entityInQueryIndex, spawner.prefab);
                                spawner.spawnedVoxelsCount += 1;
                                ecb.SetComponent(entityInQueryIndex, instance, new Translation() {
                                        Value = position
                                });
                                ecb.SetComponent(entityInQueryIndex, instance, new Voxel() {value = 1});
                                ecb.SetSharedComponent(entityInQueryIndex, instance,
                                        new VoxelStatus() {status = VoxelStatus.Status.Generated});
                            }
                        }
                    }).ScheduleParallel();

            // Entities.WithName("Spawner").ForEach((
            //         int entityInQueryIndex,
            //         ref VoxelSpawner spawner,
            //         in LocalToWorld localToWorld
            // ) => { }).WithDisposeOnCompletion(positions).ScheduleParallel();
            _beginSimulationSystem.AddJobHandleForProducer(Dependency);
        }

        private static int GetYForVoxel(int x, int z, float noiseScale) {
            var pX = x * noiseScale;
            var pZ = z * noiseScale;

            return Mathf.RoundToInt(Mathf.PerlinNoise(pX, pZ) * 256);
        }
    }
}