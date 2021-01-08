using System.Collections.Generic;
using System.Linq;
using Terrain.Voxel;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = System.Diagnostics.Debug;

namespace Terrain.ECS {
    public class VoxelMeshGeneratorSystem : SystemBase {
        private EntityQuery _voxelMeshGenerationQuery;

        protected override void OnCreate() {
            base.OnCreate();
            _voxelMeshGenerationQuery = GetEntityQuery(
                    typeof(Voxel),
                    typeof(VoxelStatus),
                    typeof(Translation),
                    typeof(RenderMesh)
            );
        }


        protected override void OnUpdate() {
            _voxelMeshGenerationQuery.SetSharedComponentFilter(
                    new VoxelStatus() {status = VoxelStatus.Status.Generated});

            if (_voxelMeshGenerationQuery.IsEmpty) {
                return;
            }


            var entities = _voxelMeshGenerationQuery.ToEntityArray(Allocator.TempJob);
            var voxels = _voxelMeshGenerationQuery.ToComponentDataArray<Voxel>(Allocator.TempJob);
            var translations = _voxelMeshGenerationQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

            foreach (var entity in entities) {
                var translation = GetComponent<Translation>(entity);

                var verts = new NativeList<Vector3>(Allocator.TempJob);
                var tris = new NativeList<int>(Allocator.TempJob);

                MakeCube(voxels, translations, new int3(translation.Value), verts, tris);

                var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(entity);
                var vertices = verts.ToArray();
                var triangles = tris.ToArray();

                var mesh = new UnityEngine.Mesh {
                        vertices = vertices,
                        triangles = triangles,
                };
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                mesh.Optimize();
                var newRenderMesh = new RenderMesh() {
                        mesh = mesh,
                        material = renderMesh.material
                };
                EntityManager.SetSharedComponentData(entity, newRenderMesh);

                EntityManager.SetComponentData(entity, new RenderBounds {
                        Value = mesh.bounds.ToAABB()
                });
                EntityManager.SetSharedComponentData(entity, new VoxelStatus() {
                        status = VoxelStatus.Status.Rendered,
                });
                verts.Dispose();
                tris.Dispose();
            }

            entities.Dispose();
            voxels.Dispose();
            translations.Dispose();
        }


        private static int GetNeighbor(NativeArray<Voxel> voxels, NativeArray<Translation> translations, int3 position,
                Direction dir) {
            var offsetToCheck = VoxelUtils.Offsets[(int) dir];
            var neighborCoord =
                    new VoxelUtils.DataCoordinate(position.x + offsetToCheck.x, position.y + offsetToCheck.y,
                            position.z + offsetToCheck.z);

            var index = translations.Select(t => new int3(t.Value)).ToList().FindIndex(p =>
                    p.x == neighborCoord.x && p.y == neighborCoord.y && p.z == neighborCoord.z);

            return index == -1 ? 0 : voxels.ElementAt(index).value;
        }

        private static void MakeCube(NativeArray<Voxel> voxels, NativeArray<Translation> translations,
                int3 position,
                NativeList<Vector3> vertices, NativeList<int> triangles) {
            for (var i = 0; i < 6; i++) {
                var dir = (Direction) i;
                if (GetNeighbor(voxels, translations, position, dir) == 0) {
                    MakeFace(dir, position, vertices, triangles);
                }
            }
        }

        private static void MakeFace(Direction dir, int3 position, NativeList<Vector3> vertices,
                NativeList<int> triangles) {
            var verts = BasicMeshData.CubeMeshData.FaceVertices(dir, position);
            foreach (var vert in verts) {
                vertices.Add(vert);
            }

            var vCount = vertices.Length;

            triangles.Add(vCount - 4);
            triangles.Add(vCount - 4 + 1);
            triangles.Add(vCount - 4 + 2);
            triangles.Add(vCount - 4);
            triangles.Add(vCount - 4 + 2);
            triangles.Add(vCount - 4 + 3);
        }
    }
}