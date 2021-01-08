using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Terrain.ECS {
    public class ChunkSpawnerAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity {
        [SerializeField] private GameObject prefab;
        [SerializeField] private float maxDistanceFromSpawner;

        public static Material material;
        [SerializeField] private Material _material;

        private void Awake() {
            material = _material;
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add(prefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new VoxelSpawner() {
                    prefab = conversionSystem.GetPrimaryEntity(prefab),
                    maxDistanceFromSpawner = maxDistanceFromSpawner,
                    maxVoxels = 1024,
            });
        }
    }
}