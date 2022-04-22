using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GeneratePark))]
public class GenerateParkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GeneratePark gen = (GeneratePark)target;
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
