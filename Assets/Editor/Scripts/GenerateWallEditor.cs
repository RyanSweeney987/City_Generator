using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerateWall))]
public class GenerateWallEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GenerateWall gen = (GenerateWall)target;
        if (GUILayout.Button("Generate Wall"))
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
