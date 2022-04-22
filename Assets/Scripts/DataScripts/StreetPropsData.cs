using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Data/Street Prop", order = 3)]
public class StreetPropsData : ScriptableObject
{
    public GameObject prefab;
    public StreetAssetType type;
}

public enum StreetAssetType
{
    Lamp,
    Bench,
    Car
}
