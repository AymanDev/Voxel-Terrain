using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Terrain))]
public class TerrainEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var terrain = (Terrain) target;

        // if (GUILayout.Button("Update terrain")) {
            // terrain.Start();
        // }
    }
}