using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Data/Industry Prop", order = 3)]
public class IndustryPropsData : ScriptableObject
{
    public GameObject prefab;
    public IndustryAssetType type;
}

public enum IndustryAssetType
{
    Wall,
    WallEnd,
    WallGate
} 