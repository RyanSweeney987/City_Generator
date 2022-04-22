using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerateCity))]
public class GenerateCityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GenerateCity gen = (GenerateCity)target;
        if (GUILayout.Button("Generate City"))
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