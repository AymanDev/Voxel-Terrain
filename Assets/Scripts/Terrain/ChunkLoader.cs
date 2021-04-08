using UnityEngine;
using UnityEngine.Events;

namespace Terrain {
    [RequireComponent(typeof(SphereCollider))]
    public class ChunkLoader : MonoBehaviour {
        [SerializeField] private int chunkLoadDistance;
        [SerializeField] private int chunkUnloadDistance;
        [SerializeField] private new string tag = "Chunk";
        [SerializeField] private UnityEvent<Chunk> onChunkExitRange;
        private SphereCollider _trigger;

        private void Start() {
            _trigger = GetComponent<SphereCollider>();
            _trigger.isTrigger = true;
            _trigger.radius = chunkUnloadDistance * TerrainManager.instance.config.chunkSize;

            TerrainManager.instance.SpawnChunksAround(transform.localPosition, chunkLoadDistance);
        }

        private void OnTriggerExit(Collider other) {
            if (!other.CompareTag(tag)) {
                return;
            }

            var chunk = other.GetComponent<Chunk>();
            onChunkExitRange.Invoke(chunk);

            if (chunk.IsChunkActive) {
                TerrainManager.instance.MoveChunkToCachePool(chunk);
            }

            TerrainManager.instance.SpawnChunksAround(transform.localPosition, chunkLoadDistance);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!TerrainManager.instance) {
                return;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.localPosition,
                chunkLoadDistance * TerrainManager.instance.config.chunkSize);
        }
#endif
    }
}