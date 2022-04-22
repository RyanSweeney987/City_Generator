using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseGenerator
{
    public List<HouseComponentData> houseComponents = new List<HouseComponentData>();
    public Material wireMaterial;

    public GameObject GenerateHouse(int width, int storeys, bool generateBigPower)
    {
        List<HouseComponentData> doors = houseComponents.FindAll(_ => _.type == HouseComponentType.Door);
        List<HouseComponentData> walls = houseComponents.FindAll(_ => _.type == HouseComponentType.Wall);
        List<HouseComponentData> windows = houseComponents.FindAll(_ => _.type == HouseComponentType.Window);
        List<HouseComponentData> rooves = houseComponents.FindAll(_ => _.type == HouseComponentType.Roof);
        List<HouseComponentData> roofWindows = houseComponents.FindAll(_ => _.type == HouseComponentType.RoofWindow);
        List<HouseComponentData> roofPowerStorage = houseComponents.FindAll(_ => _.type == HouseComponentType.RoofPowerStorage);
        List<HouseComponentData> power = houseComponents.FindAll(_ => _.type == HouseComponentType.Power);
        List<HouseComponentData> wallPowerStorage = houseComponents.FindAll(_ => _.type == HouseComponentType.WallPowerStorage);
        List<HouseComponentData> chimney = houseComponents.FindAll(_ => _.type == HouseComponentType.Chimney);
        List<HouseComponentData> largePowerStorage = houseComponents.FindAll(_ => _.type == HouseComponentType.LargePowerStorage);

        //Debug.Log("Doors: " + doors.Count);
        //Debug.Log("Walls: " + walls.Count);
        //Debug.Log("Windows: " + windows.Count);
        //Debug.Log("Rooves: " + rooves.Count);
        //Debug.Log("Roof Power Storage: " + roofPowerStorage.Count);
        //Debug.Log("Power: " + power.Count);
        //Debug.Log("Power Storage: " + wallPowerStorage.Count);

        GameObject house = new GameObject();

        GameObject[,,] houseGameObjects = new GameObject[storeys + 1, 2, width];

        bool hasFrontDoor = false;
        bool hasBackDoor = false;
        bool hasPower = false;
        bool hasChimney = false;
        bool hasWallPower = false;

        Vector3 backOffset = new Vector3(0f, 0f, -2f);
        Vector3 heightOfsset = new Vector3(0f, 2f, 0f);
        Vector3 widthOffset = new Vector3(2f, 0f ,0f);
        Vector3 backRotation = new Vector3(0f, 180f, 0f);

        GameObject powerObj = GameObject.Instantiate(power[0].prefab);
        GameObject frontDoor = GameObject.Instantiate(doors[0].prefab);

        width = generateBigPower ? width - 1 : width; // Remove width to add room for large side power

        // The blocks are set like voxels, so 3D
        for (int i = 0; i < storeys + 1; i++) // Height
        {
            for(int j = 0; j < 2; j++) // Depth is always 2
            {
                for(int k = 0; k < width; k++) // Width of house looking at from street
                {
                    // Front door
                    if(i == 0 && !hasFrontDoor)
                    {
                        frontDoor.transform.parent = house.transform;

                        houseGameObjects[i, j, k] = frontDoor;
                        hasFrontDoor = true;
                    } else if(i == storeys && j == 1 && !hasPower) // Power set position
                    {
                        powerObj.transform.parent = house.transform;
                        powerObj.transform.localPosition = (heightOfsset * storeys) + (backOffset * j);
                        powerObj.transform.localRotation = Quaternion.Euler(backRotation);

                        houseGameObjects[i, j, k] = powerObj;
                        hasPower = true;
                    } else if(i == storeys) { // Roof General
                        GameObject prefab = UtilityFunctions.GetWeightedRandom(new List<(float weight, GameObject gameObject)> {
                            (0.5f, rooves[0].prefab),
                            (0.2f, roofWindows[0].prefab),
                            (0.15f, chimney[0].prefab),
                            (0.15f, roofPowerStorage[0].prefab)
                        });
                        GameObject go = GameObject.Instantiate(prefab);
                        go.transform.parent = house.transform;
                        go.transform.localPosition = (heightOfsset * storeys) + (backOffset * j) + (widthOffset * k);
                        if(j == 1)
                        {
                            go.transform.localRotation = Quaternion.Euler(backRotation);
                        }
                        
                        houseGameObjects[i, j, k] = go;
                    } else // Walls general
                    {
                        GameObject prefab = UtilityFunctions.GetWeightedRandom(new List<(float weight, GameObject gameObject)> {
                            (0.95f, windows[0].prefab),
                            (0.05f, wallPowerStorage[0].prefab)
                        });
                        GameObject go = GameObject.Instantiate(prefab);
                        go.transform.parent = house.transform;
                        go.transform.localPosition = (heightOfsset * i) + (backOffset * j) + (widthOffset * k);
                        if (j == 1)
                        {
                            go.transform.localRotation = Quaternion.Euler(backRotation);
                        }

                        houseGameObjects[i, j, k] = go;
                    }
                }
            }
        }

        if(generateBigPower)
        {
            GameObject bigPower = GameObject.Instantiate(largePowerStorage[0].prefab);
            bigPower.transform.parent = house.transform;
            bigPower.transform.localPosition = (widthOffset * width) + new Vector3(0f, 0f, -1f);
        }

        return house;
    }

    public GameObject CreatePowerLine(GameObject house)
    {
        WireConnection connection = house.GetComponentInChildren<WireConnection>();

        List<GameObject> transmitterTowers = new List<GameObject>(GameObject.FindGameObjectsWithTag("PowerTransmitter"));

        GameObject nearestPowerTransmitter = null;
        float minDistance = 100000f;
        foreach (GameObject go in transmitterTowers)
        {
            float distance = Vector3.Distance(connection.transform.position, go.transform.position);
            if (distance < minDistance)
            {
                nearestPowerTransmitter = go;
                minDistance = distance;
            }
        }

        GameObject powerLine = new GameObject();
        powerLine.name = "Power Line";
        powerLine.transform.parent = house.transform;

        if (nearestPowerTransmitter != null)
        {
            LineRenderer lineRenderer = powerLine.AddComponent<LineRenderer>();

            lineRenderer.SetPosition(0, connection.GetAttachLocation());
            lineRenderer.SetPosition(1, nearestPowerTransmitter.transform.position);
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material = wireMaterial;
            lineRenderer.startColor = new Color(0.125f, 0.125f, 0.125f, 1f);
            lineRenderer.endColor = new Color(0.125f, 0.125f, 0.125f, 1f);
        }

        return powerLine;
    }
}