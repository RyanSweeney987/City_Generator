using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateStreet : MonoBehaviour
{
    [Header("Street Generation")]
    public List<Vector2> streetPoints;
    public float streetWidth = 1f;
    public Material streetMaterial;
    public bool connectLoop = false;

    [Header("Lamp Generation")]
    public bool generateLamps = false;
    public float lampDistance = 5f;
    public GameObject lampPrefab;

    [Header("Bench Generation")]
    public bool generateBenches = false;
    public GameObject benchPrefab;

    [Header("House Generation")]
    public bool generateHouses = false;
    public int minWidth = 2;
    public int maxWidth = 4;
    public int minHeight = 2;
    public int maxHeight = 4;
    public List<HouseComponentData> houseComponents;
    public Material wireMaterial;

    private GameObject streets;

    public void Generate()
    {
        Clear();

        streets = new GameObject();
        streets.name = "Streets";

        List<GameObject> streetsList = new List<GameObject>();
        StreetGenerator generator = new StreetGenerator();

        float halfStreetWidth = streetWidth / 2f;

        int loopCount = connectLoop ? streetPoints.Count : streetPoints.Count - 1;
        for (int i = 0; i < loopCount; i++)
        {
            Vector2 start = streetPoints[i];
            Vector2 end = i == streetPoints.Count - 1 ? streetPoints[0] : streetPoints[i + 1];

            GameObject street = generator.GenerateStreet(start, end, streetMaterial, streetWidth);
            street.transform.parent = streets.transform;
            street.name = "Street " + (i + 1);

            streetsList.Add(street);
        }

        if(generateLamps)
        {
            float zOffset = 1f - halfStreetWidth;
            float xOffset = halfStreetWidth - 2f;

            for (int i = 0; i < streetsList.Count; i++)
            {
                Vector2 start = streetPoints[i];
                Vector2 end = i == streetPoints.Count - 1 ? streetPoints[0] : streetPoints[i + 1];

                float distance = (end - start).magnitude;
                int blockCount = Mathf.FloorToInt(distance / lampDistance);

                for(int j = 0; j < blockCount; j++)
                {
                    GameObject streetLamp = Instantiate(lampPrefab);
                    streetLamp.transform.parent = streetsList[i].transform;
                    streetLamp.transform.localPosition = new Vector3(xOffset + lampDistance * j, 0f, zOffset);
                }
            }
        }

        if(generateBenches)
        {
            float zOffset = 0.1f - halfStreetWidth;
            float xOffset = halfStreetWidth;

            for(int i = 0; i < streetsList.Count; i++)
            {
                // Start of street
                GameObject startBench = Instantiate(benchPrefab);
                startBench.transform.parent = streetsList[i].transform;
                startBench.transform.localPosition = new Vector3(xOffset, 0f, zOffset);
                startBench.transform.localRotation = Quaternion.identity;

                Vector2 start = streetPoints[i];
                Vector2 end = i == streetPoints.Count - 1 ? streetPoints[0] : streetPoints[i + 1];

                float distance = (end - start).magnitude - xOffset;
                // End of street
                GameObject endBench = Instantiate(benchPrefab);
                endBench.transform.parent = streetsList[i].transform;
                endBench.transform.localPosition = new Vector3(distance, 0f, zOffset);
                endBench.transform.localRotation = Quaternion.identity;
            }
        }

        if (generateHouses)
        {
            for(int i = 0; i < streetsList.Count; i++)
            {
                Vector2 start = streetPoints[i];
                Vector2 end = i == streetPoints.Count - 1 ? streetPoints[0] : streetPoints[i + 1];

                float blockWidth = 2f;
                float distance = (end - start).magnitude - (streetWidth * 2f);
                int blockCount = Mathf.FloorToInt(distance / blockWidth);

                HouseGenerator houseGenerator = new HouseGenerator();
                houseGenerator.houseComponents = houseComponents;
                houseGenerator.wireMaterial = wireMaterial;

                int currentBlockCount = 0;
                int houseCount = 0;
                while(currentBlockCount < blockCount)
                {
                    int height = Mathf.FloorToInt(Random.Range(minHeight, maxHeight + 1));
                    int width = Mathf.FloorToInt(Random.Range(minWidth, maxWidth + 1));

                    if(blockCount - currentBlockCount < minWidth)
                    {
                        break;
                    }

                    if(blockCount - (currentBlockCount + width) < 0)
                    {
                        width = blockCount - currentBlockCount;
                    }

                    GameObject house = houseGenerator.GenerateHouse(width, height, width == minWidth ? false : Random.value > 0.9f);
                    house.name = "House " + houseCount++;
                    house.transform.parent = streetsList[i].transform;
                    house.transform.localPosition = GetHouseOffset(currentBlockCount);
                    house.transform.localRotation = Quaternion.identity;
                    houseGenerator.CreatePowerLine(house);

                    currentBlockCount += width;
                }
            }
        }
    }

    // Offset from street and along the street
    private Vector3 GetHouseOffset(int currentWidth)
    {
        float houseNumberOffset = 2f * currentWidth;
        float halfStreet = streetWidth / 2f;
        return new Vector3(1f + streetWidth + houseNumberOffset, 0f, -halfStreet -1f);
    }

    public void Clear()
    {
        if(streets != null && streets.activeInHierarchy)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(streets);
            }
            else if (Application.isPlaying)
            {
                Destroy(streets);
            }
        }
    }

    private void OnValidate()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        List<Vector3> points = streetPoints.ConvertAll(_ => new Vector3(_.x, 0f, _.y));
        for(int i = 0; i < points.Count - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
    }
}
