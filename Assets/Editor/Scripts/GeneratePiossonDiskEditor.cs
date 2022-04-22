using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GeneratePiossonDisk))]
public class GeneratePiossonDiskEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GeneratePiossonDisk gen = (GeneratePiossonDisk)target;
        if (GUILayout.Button("Generate Street"))
        {
            gen.Clear();
            gen.Generate();
        }

        if (GUILayout.Button("Clear"))
        {
            gen.Clear();
        }

        DrawDefaultInspector();
    }
}
