using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Data/Park Prop", order = 4)]
public class ParkPropsData : ScriptableObject
{
    public GameObject prefab;
    public ParkAssetType type;
}

public enum ParkAssetType
{
    Fountain,
    Tree,
    Rock,
    Special
}