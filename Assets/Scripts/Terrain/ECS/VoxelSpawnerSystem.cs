using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;

namespace Terrain.ECS {
    public class VoxelSpawnerSystem : SystemBase {
        private BeginSimulationEntityCommandBufferSystem _beginSimulationSystem;
        private EntityQuery _voxelQuery;
        private EntityQuery _spawnerQuery;

        protected override void OnCreate() {
            base.OnCreate();
            _beginSimulationSystem =
                    World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
            _voxelQuery = GetEntityQuery(
                    ComponentType.ReadOnly<Voxel>(),
                    ComponentType.ReadOnly<Translation>()
            );
            _spawnerQuery = GetEntityQuery(
                    typeof(VoxelSpawner),
                    ComponentType.Exclude<VoxelSpawnerDoneTag>()
            );
        }


        protected override void OnUpdate() {
            if (_spawnerQuery.IsEmpty) {
                return;
            }

            Profiler.BeginSample("System preparations");
            var ecb = _beginSimulationSystem.CreateCommandBuffer().AsParallelWriter();
            var positions = _voxelQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            Profiler.EndSample();

            Entities
                    .WithNone<VoxelSpawnerDoneTag>()
                    .WithReadOnly(positions)
                    .WithDisposeOnCompletion(positions)
                    .WithoutBurst()
                    .ForEach((
                            Entity spawnerEntity,
                            int entityInQueryIndex,
                            ref VoxelSpawner spawner,
                            in LocalToWorld localToWorld
                    ) => {
                        // var positionType = GetComponentTypeHandle<Translation>();

                        Profiler.BeginSample("Spawning voxels");
                        for (var x = -spawner.maxDistanceFromSpawner; x < spawner.maxDistanceFromSpawner; x++) {
                            for (var z = -spawner.maxDistanceFromSpawner; z < spawner.maxDistanceFromSpawner; z++) {
                                var position = new int3(localToWorld.Position +
                                                        new float3(x, 0, z));
                                if (positions.Any(p => p.Value.Equals(position))) {
                                    continue;
                                }

                                position.y = GetYForVoxel(position.x, position.z, 0.01f);

                                var instance = ecb.Instantiate(entityInQueryIndex, spawner.prefab);
                                ecb.SetComponent(entityInQueryIndex, instance, new Translation() {
                                        Value = position
                                });
                                ecb.SetComponent(entityInQueryIndex, instance, new Voxel() {value = 1});
                                ecb.SetSharedComponent(entityInQueryIndex, instance,
                                        new VoxelStatus() {status = VoxelStatus.Status.Generated});
                            }
                        }

                        ecb.AddComponent(entityInQueryIndex, spawnerEntity, new VoxelSpawnerDoneTag());

                        Profiler.EndSample();
                    }).ScheduleParallel();
            _beginSimulationSystem.AddJobHandleForProducer(Dependency);
        }

        private static int GetYForVoxel(int x, int z, float noiseScale) {
            var pX = x * noiseScale;
            var pZ = z * noiseScale;

            return Mathf.RoundToInt(Mathf.PerlinNoise(pX, pZ) * 256);
        }
    }
}