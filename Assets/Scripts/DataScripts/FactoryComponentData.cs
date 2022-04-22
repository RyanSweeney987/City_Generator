using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Data/Factory Component", order = 2)]
public class FactoryComponentData : BuildingComponentData
{
    public GameObject prefab;
    public FactoryComponentType type = FactoryComponentType.Wall;
}

public enum FactoryComponentType
{
    Wall,
    Door,
    Window,
    Power,
    SmokeStack,
    Storage,
    Resource
}