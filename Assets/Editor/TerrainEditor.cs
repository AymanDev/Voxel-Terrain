using Terrain;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainManager))]
public class TerrainEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var terrain = (TerrainManager) target;

        GUILayout.Label("Maximum possible chunks: " + terrain.config.MaximumChunksCount);
        GUILayout.Label("Active chunks: " + terrain.ActiveChunksCount);
        GUILayout.Label("Cached chunks: " + terrain.CachedChunksCount);
        GUILayout.Label("Ready to load chunks: " + terrain.ReadyToLoadChunksCount);
        GUILayout.Label("Loading chunks: " + terrain.LoadingChunksCount);


        // if (GUILayout.Button("Load chunks")) {
        // terrain.StartJobLoadingChunk();
        // }

        // GUILayout.Label("Chunks are loading: " + terrain.chunksAreLoading);
        // GUILayout.Label("Jobs are completed: " + terrain.jobsCompleted);
    }
    
    
}