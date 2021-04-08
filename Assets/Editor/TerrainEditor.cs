using Terrain;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainManager))]
public class TerrainEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var terrain = (TerrainManager) target;



        // if (GUILayout.Button("Load chunks")) {
        // terrain.StartJobLoadingChunk();
        // }

        // GUILayout.Label("Chunks are loading: " + terrain.chunksAreLoading);
        // GUILayout.Label("Jobs are completed: " + terrain.jobsCompleted);
    }
    
    
}