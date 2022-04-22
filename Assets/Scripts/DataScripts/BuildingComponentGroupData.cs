using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Data/Building Component Group", order = 1)]
public class BuildingComponentGroupData : ScriptableObject
{
    public List<BuildingComponentData> components;
    public BuildingGroupType groupType = BuildingGroupType.Residential;
}

public enum BuildingGroupType
{
    Residential,
    Industrial,
    Park
}