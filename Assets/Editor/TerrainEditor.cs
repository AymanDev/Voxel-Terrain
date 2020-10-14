using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Terrain))]
public class TerrainEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var terrain = (Terrain) target;

        GUILayout.Label("Maximum possible chunks: " + terrain.MaximumChunksCount);
        GUILayout.Label("Active chunks: " + terrain.ActiveChunksCount);
        GUILayout.Label("Cached chunks: " + terrain.CachedChunksCount);
    }
}