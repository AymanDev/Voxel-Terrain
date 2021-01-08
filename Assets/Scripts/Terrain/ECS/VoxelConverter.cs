using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terrain.ECS {
    public class VoxelConverter : MonoBehaviour, IConvertGameObjectToEntity {
        [SerializeField] private Material material;
        [SerializeField] private UnityEngine.Mesh mesh;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            
            dstManager.AddComponentData(entity, new Voxel() {
                    value = 0
            });
            dstManager.AddSharedComponentData(entity, new VoxelStatus() {
                    status = VoxelStatus.Status.Spawned
            });
            dstManager.AddComponentData(entity, new RenderBounds() {
                    Value = mesh.bounds.ToAABB()
            });
            dstManager.AddSharedComponentData(entity, new RenderMesh() {
                    material = material,
                    mesh = mesh
            });
        }
    }
}