using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BullshitSaveDataGenerator))]
public class BullshitSaveDataGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        var generator = (BullshitSaveDataGenerator)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Bullshit Save Data")) {
            generator.GenerateBullshitSaveData();
        }

        if (GUILayout.Button("Clear Save Data")) {
            generator.ClearSaveData();
        }
    }
}
