using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerateFactory))]
public class GenerateFactoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GenerateFactory gen = (GenerateFactory)target;
        if (GUILayout.Button("Generate Factory"))
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
