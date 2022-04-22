using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Data/House Component", order = 2)]
public class HouseComponentData : BuildingComponentData
{
    public GameObject prefab;
    public HouseComponentType type = HouseComponentType.Wall;
}

public enum HouseComponentType
{
    Wall,
    Door,
    Window,
    Roof,
    RoofWindow,
    Chimney,
    Power,
    RoofPowerStorage,
    WallPowerStorage,
    LargePowerStorage
}
