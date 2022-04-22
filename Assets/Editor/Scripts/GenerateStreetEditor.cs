using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerateStreet))]
public class GenerateStreetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GenerateStreet gen = (GenerateStreet)target;
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