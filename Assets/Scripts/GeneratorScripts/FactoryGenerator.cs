using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryGenerator
{
    public List<FactoryComponentData> factoryComponents = new List<FactoryComponentData>();
    public Material wireMaterial;

    public GameObject GenerateFactory(int minWidth, int maxWidth)
    {
        GameObject factory = new GameObject();

        List<FactoryComponentData> doors = factoryComponents.FindAll(_ => _.type == FactoryComponentType.Door);
        List<FactoryComponentData> walls = factoryComponents.FindAll(_ => _.type == FactoryComponentType.Wall);
        List<FactoryComponentData> windows = factoryComponents.FindAll(_ => _.type == FactoryComponentType.Window);
        List<FactoryComponentData> power = factoryComponents.FindAll(_ => _.type == FactoryComponentType.Power);
        List<FactoryComponentData> smokeStacks = factoryComponents.FindAll(_ => _.type == FactoryComponentType.SmokeStack);
        List<FactoryComponentData> resources = factoryComponents.FindAll(_ => _.type == FactoryComponentType.Resource);
        List<FactoryComponentData> storages = factoryComponents.FindAll(_ => _.type == FactoryComponentType.Storage);

        int width = Mathf.FloorToInt(Random.Range(minWidth, maxWidth));

        GameObject[,] factorGameObject = new GameObject[2, width];

        int doorCount = 0;
        bool hasPower = false;

        Vector3 backOffset = new Vector3(0f, 0f, -4f);
        Vector3 widthOffset = new Vector3(4f, 0f, 0f);
        Vector3 backRotation = new Vector3(0f, 180f, 0f);

        // The blocks are set like voxels, so 3D
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if(j == width - 1 && doorCount < 2)
                {
                    GameObject frontDoor = GameObject.Instantiate(doors[0].prefab);
                    frontDoor.transform.parent = factory.transform;
                    frontDoor.transform.localPosition = backOffset * i + widthOffset * j;
                    frontDoor.transform.localRotation = Quaternion.Euler(i * backRotation);

                    factorGameObject[i, j] = frontDoor;
                    doorCount++;
                }
                else if(j == 1 && i == 1 && !hasPower)
                {
                    GameObject powerObj = GameObject.Instantiate(power[0].prefab);
                    powerObj.transform.parent = factory.transform;
                    powerObj.transform.localPosition = backOffset * i + widthOffset * j;
                    powerObj.transform.rotation = Quaternion.Euler(backRotation);

                    factorGameObject[i, j] = powerObj;
                    hasPower = true;
                } else if(j == 0 && Random.value < 0.8f)
                {
                    GameObject smokeStack = GameObject.Instantiate(smokeStacks[0].prefab);
                    smokeStack.transform.parent = factory.transform;
                    smokeStack.transform.position = backOffset * i;
                } else
                {
                    GameObject prefab = UtilityFunctions.GetWeightedRandom(new List<(float weight, GameObject gameObject)> {
                        (0.9f, windows[0].prefab),
                        (0.1f, walls[0].prefab)
                    });

                    GameObject go = GameObject.Instantiate(prefab);
                    go.transform.parent = factory.transform;
                    go.transform.localPosition = (backOffset * i) + (widthOffset * j);
                    if (i == 1)
                    {
                        go.transform.localRotation = Quaternion.Euler(backRotation);
                    }

                    factorGameObject[i, j] = go;
                }
            }
        }

        widthOffset = new Vector3(0f, 0f, -8f);
        backOffset = new Vector3(2f, 0f, -2f);
        // Storage areas - 2

        

        for (int i = 0; i < 2; i++)
        {
            GameObject prefab = UtilityFunctions.GetWeightedRandom(new List<(float weight, GameObject item)>
            {
                (1f / 3f, storages[0].prefab),
                (1f / 3f, storages[1].prefab),
                (1f / 3f, resources[0].prefab),
            });

            GameObject storage = GameObject.Instantiate(prefab);
            storage.transform.parent = factory.transform;
            storage.transform.localPosition = (widthOffset * i) + widthOffset + backOffset;
        }

        return factory;
    }

    public GameObject CreatePowerLine(GameObject factory)
    {
        WireConnection connection = factory.GetComponentInChildren<WireConnection>();

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
        powerLine.transform.parent = factory.transform;

        if (nearestPowerTransmitter != null)
        {
            LineRenderer lineRenderer = powerLine.AddComponent<LineRenderer>();

            lineRenderer.SetPosition(0, connection.GetAttachLocation());
            lineRenderer.SetPosition(1, nearestPowerTransmitter.transform.position);
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = wireMaterial;
            lineRenderer.startColor = new Color(0.125f, 0.125f, 0.125f, 1f);
            lineRenderer.endColor = new Color(0.125f, 0.125f, 0.125f, 1f);
        }

        return powerLine;
    }
}
