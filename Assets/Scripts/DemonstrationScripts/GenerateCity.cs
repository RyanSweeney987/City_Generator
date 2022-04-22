using DelaunayTriangulator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCity : MonoBehaviour
{
    [Header("City options")]
    public float citySize = 100f;
    public float minRadius = 20f;
    public float maxRadius = 30f;
    public Vector2 offset = Vector2.zero;
    public float residentialWeight = 0.8f;
    public float parkWeight = 0.1f;
    public float industryWeight = 0.1f;

    [Header("Street options")]
    public float streetWidth = 3f;

    [Header("Houses options")]
    public List<HouseComponentData> houseComponents = new List<HouseComponentData>();
    public int minHouseHeight = 1;
    public int maxHouseHeight = 3;
    public int minHouseWidth = 2;
    public int maxHouseWidth = 3;

    [Header("Industry options")]
    public List<FactoryComponentData> factoryComponentsData = new List<FactoryComponentData>();
    public float industryWallInsetAmount = -1f;
    public int minWidth = 5;
    public int maxWidth = 8;
    public List<IndustryPropsData> industryPropsPrefabs = new List<IndustryPropsData>();


    [Header("Park Prefabs")]
    public List<ParkPropsData> parkPropsPrefabs = new List<ParkPropsData>();

    [Header("Street Prefabs")]
    public List<StreetPropsData> streetPropPrefabs = new List<StreetPropsData>();

    [Header("General Prefabs")]
    public GameObject towerPrefab = null;

    [Header("Materials")]
    public Material streetMaterial = null;
    public Material residentialGround = null;
    public Material industrialGround = null;
    public Material parkGround = null;
    public Material wireMaterial = null;

    // Debug
    [Header("Debug")]
    public bool drawPoissonPoints = false;
    public bool drawDelaunayTriangulation = false;
    public bool drawCircumCircles = false;
    public bool drawVoronoiDiagram = false;
    public bool drawVoronoiCellCentroids = false;
    public bool drawVoronoiCellTriangulation = false;
    public int drawFor = -1;

    private GameObject city;
    private VoronoiGraph voronoiGraph;
    private List<CitySector> citySectors = new List<CitySector>();

    

    public void Update()
    {
        
    }

    public void FixedUpdate()
    {
        
    }

    public void Generate()
    {
        Clear();

        city = new GameObject();
        city.name = "City";

        voronoiGraph = new VoronoiGraph().GenerateVoronoiGraph(citySize, minRadius, maxRadius, offset);

        int regionCount = 0;
        foreach (VoronoiRegion region in voronoiGraph.regions)
        {
            BuildingGroupType sectorType = UtilityFunctions.GetWeightedRandom(new List<(float weight, BuildingGroupType item)>
            {
                (residentialWeight, BuildingGroupType.Residential),
                (parkWeight, BuildingGroupType.Park),
                (industryWeight, BuildingGroupType.Industrial)
            });

            CitySector sector = CitySectorFactory.CreateCitySector(region, sectorType, "Region " + regionCount++);
            sector.regionGameObject.transform.parent = city.transform;
            List<Vector2> streetPoints = UtilityFunctions.SortVectorsByAngle(region.edgePoints, region.boundsCenter, false);
            GameObject streets = sector.GenerateStreets(streetPropPrefabs, streetPoints, streetMaterial, streetWidth, true, true, 15f, true);

            Material groundMat = null;
            switch(sectorType)
            {
                case BuildingGroupType.Park:
                    groundMat = parkGround;
                    break;
                case BuildingGroupType.Residential:
                    groundMat = residentialGround;
                    break;
                case BuildingGroupType.Industrial:
                    groundMat = industrialGround;
                    break;
                default:
                    throw new System.NotSupportedException("Building group type -" + sectorType.ToString() + "- is not support");
            }

            sector.GenerateGround(groundMat);

            List<GameObject> streetList = new List<GameObject>();
            foreach(Transform child in streets.transform)
            {
                streetList.Add(child.gameObject);
            }

            switch (sectorType)
            {
                case BuildingGroupType.Park:
                    if(sector is ParkSector)
                    {
                        bool hasFountain = Random.value > 0.8f;
                        (sector as ParkSector).Generate(hasFountain, streetPoints, parkPropsPrefabs, streetMaterial, 12.5f);
                    } else
                    {
                        throw new System.InvalidCastException("Expected ParkSector, got - " + sector.GetType().Name);
                    }
                    break;
                case BuildingGroupType.Residential:
                    if (sector is ResidentialSector)
                    {
                        (sector as ResidentialSector).GeneratePowerTower(towerPrefab);
                        (sector as ResidentialSector).Generate(streetList, streetPoints, streetWidth, houseComponents, wireMaterial, parkPropsPrefabs, minHouseHeight, maxHouseHeight, minHouseWidth, maxHouseWidth);
                    }
                    else
                    {
                        throw new System.InvalidCastException("Expected Residential, got - " + sector.GetType().Name);
                    }
                    break;
                case BuildingGroupType.Industrial:
                    if (sector is IndustrialSector)
                    {
                        (sector as IndustrialSector).GeneratePowerTower(towerPrefab);
                        (sector as IndustrialSector).Generate(streetList, streetPoints, streetWidth, factoryComponentsData, industryPropsPrefabs, industryWallInsetAmount, wireMaterial, minWidth, maxWidth);
                    }
                    else
                    {
                        throw new System.InvalidCastException("Expected Industrial, got - " + sector.GetType().Name);
                    }
                    break;
                default:
                    throw new System.NotSupportedException("Building group type -" + sectorType.ToString() + "- is not supported");
            }
        }
    }

    public void Clear()
    {
        if (Application.isEditor)
        {
            if(city != null)
            {
                DestroyImmediate(city.gameObject);
            }
        }
        else if (Application.isPlaying)
        {
            if(city != null)
            {
                Destroy(city.gameObject);
            }
        }
    }

    void OnDrawGizmos()
    {
        if(voronoiGraph != null)
        {
            if(voronoiGraph.points != null)
            {
                if (drawPoissonPoints)
                {
                    // Display the explosion radius when selected
                    foreach (Vector2 elem in voronoiGraph.points)
                    {
                        // Centre
                        Gizmos.color = new Color(1.0f, 0.0f, 0f, 1.0f);
                        Gizmos.DrawWireSphere(new Vector3(elem.x, 0.0f, elem.y), 1f);
                        // Radius
                        Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 1.0f);
                        Gizmos.DrawWireSphere(new Vector3(elem.x, 0.0f, elem.y), minRadius);
                        // Max radius
                        //Gizmos.color = new Color(0.0f, 0.0f, 1.0f, 1.0f);
                        //Gizmos.DrawWireSphere(new Vector3(elem.x, 0.0f, elem.y), 2.0f);
                    }
                }
            }

            if(voronoiGraph.regions != null)
            {
                int count = 0;
                foreach (VoronoiRegion elem in voronoiGraph.regions)
                {
                    if(drawFor > -1 && count != drawFor)
                    {
                        count++;
                        continue;
                    }

                    if (drawVoronoiCellCentroids)
                    {
                        elem.DrawRegionCircumCenter();
                    }

                    if (drawVoronoiCellTriangulation)
                    {
                        Gizmos.color = new Color(0.75f, 0.25f, 0.0f, 1.0f);

                        foreach(DelaunayTriangulator.Triad triangle in elem.regionTriangles)
                        {
                            Vector3 start = new Vector3(triangle.va.x, 0f, triangle.va.y);
                            Vector3 end = new Vector3(triangle.vb.x, 0f, triangle.vb.y);

                            Gizmos.DrawLine(new Vector3(triangle.va.x, 0f, triangle.va.y), new Vector3(triangle.vb.x, 0f, triangle.vb.y));
                            Gizmos.DrawLine(new Vector3(triangle.vb.x, 0f, triangle.vb.y), new Vector3(triangle.vc.x, 0f, triangle.vc.y));
                            Gizmos.DrawLine(new Vector3(triangle.vc.x, 0f, triangle.vc.y), new Vector3(triangle.va.x, 0f, triangle.va.y));
                        }
                    }

                    if (drawDelaunayTriangulation)
                    {
                        elem.DrawRegionTriangles();
                    }

                    if (drawCircumCircles)
                    {
                        elem.DrawRegionTriangleCircumCenters();
                    }

                    if(drawVoronoiDiagram)
                    {
                        List<Vector2> edgePoints = elem.triangles.ConvertAll(_ => _.triangle.circumCenter);
                        edgePoints = UtilityFunctions.SortVectorsByAngle(edgePoints, UtilityFunctions.FindBoundsCenter(edgePoints).center);
                        edgePoints = UtilityFunctions.InsetShape(edgePoints, 15f);
                        List<Vector2> subDivPoints = UtilityFunctions.Subdivide(edgePoints, 2);

                        foreach (Vector2 point in subDivPoints)
                        {
                            Gizmos.color = new Color(1.0f, 0.5f, 0.5f, 1.0f);
                            Gizmos.DrawWireSphere(new Vector3(point.x, 0.0f, point.y), 1f);
                        }

                        float diameter = UtilityFunctions.FindLargestDiameter(subDivPoints);
                        Vector2 diCenter = UtilityFunctions.FindLargestDiameterMidpoint(subDivPoints);
                        float angle = UtilityFunctions.FindLargetDiameterAngle(subDivPoints);

                        Gizmos.color = new Color(1.0f, 0.5f, 0.5f, 1.0f);
                        Gizmos.DrawWireSphere(new Vector3(diCenter.x, 0.0f, diCenter.y), 1f);
                    }

                    foreach (DelaunayTriangle verts in elem.triangles)
                    {
                        if (drawVoronoiDiagram)
                        {
                            Vector3 start = new Vector3(verts.triangle.circumCenter.x, 0f, verts.triangle.circumCenter.y);

                            foreach (DelaunayTriangle neighbour in verts.neighbours)
                            {
                                Vector3 end = new Vector3(neighbour.triangle.circumCenter.x, 0.0f, neighbour.triangle.circumCenter.y);
                        
                                Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                                Gizmos.DrawLine(start, end);

                                Vector3 d = end - start;

                                Vector3 leftNormal = new Vector3(-d.z, 0f,d.x).normalized;
                                Vector3 rightNormal = new Vector3(d.z, 0f, -d.x).normalized;

                                Gizmos.color = Color.blue;
                                Gizmos.DrawLine(start, start + (leftNormal * 1f));
                                Gizmos.DrawLine(start, start + (rightNormal * 1f));
                            }
                        }
                    }
                }
            }
        }
    }
}

public abstract class CitySector {
    public VoronoiRegion voronoiRegion;
    public BuildingGroupType sectorType;
    public GameObject regionGameObject;
    public List<GameObject> carGameObjects = new List<GameObject>();

    public CitySector(VoronoiRegion region, string regionName)
    {
        this.voronoiRegion = region;
        regionGameObject = new GameObject(regionName);
    }

    //public abstract GameObject Generate();
    public void Clear()
    {
        if (regionGameObject != null && regionGameObject.activeInHierarchy)
        {
            if (Application.isEditor)
            {
                GameObject.DestroyImmediate(regionGameObject);
            }
            else
            {
                GameObject.Destroy(regionGameObject);
            }
        }
    }

    public GameObject GenerateGround(Material groundMaterial)
    {
        GameObject ground = new GameObject();
        ground.name = "Ground " + sectorType.ToString();
        ground.transform.parent = regionGameObject.transform;
        ground.transform.localPosition = new Vector3(0f, -0.01f, 0f);

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        // Find the lowest [x,y] and highest [x,y] coordinates
        foreach (Triad tri in voronoiRegion.regionTriangles)
        {
            if (tri.va.x < minX) minX = tri.va.x;
            if (tri.va.y < minY) minY = tri.va.y;
            if (tri.va.x > maxX) maxX = tri.va.x;
            if (tri.va.y > maxY) maxY = tri.va.y;

            if (tri.vb.x < minX) minX = tri.vb.x;
            if (tri.vb.y < minY) minY = tri.vb.y;
            if (tri.vb.x > maxX) maxX = tri.vb.x;
            if (tri.vb.y > maxY) maxY = tri.vb.y;

            if (tri.vc.x < minX) minX = tri.vc.x;
            if (tri.vc.y < minY) minY = tri.vc.y;
            if (tri.vc.x > maxX) maxX = tri.vc.x;
            if (tri.vc.y > maxY) maxY = tri.vc.y;
        }

        float rangeX = maxX - minX;
        float rangeY = maxY - minY;

        List<int> indeces = new List<int>();

        List<(int index, Vector3 vector, Vector2 uv)> verts = new List<(int index, Vector3 vector, Vector2 uv)>();

        foreach (Triad tri in voronoiRegion.regionTriangles)
        {
            Vector3 va = new Vector3(tri.va.x, 0f, tri.va.y);
            Vector3 vb = new Vector3(tri.vb.x, 0f, tri.vb.y);
            Vector3 vc = new Vector3(tri.vc.x, 0f, tri.vc.y);

            Vector2 uva = new Vector2((tri.va.x - minX) / rangeX, (tri.va.y - minY) / rangeY);
            Vector2 uvb = new Vector2((tri.vb.x - minX) / rangeX, (tri.vb.y - minY) / rangeY);
            Vector2 uvc = new Vector2((tri.vc.x - minX) / rangeX, (tri.vc.y - minY) / rangeY);

            (int index, Vector3 vector, Vector2 uv) vertA = (tri.a, va, uva);
            (int index, Vector3 vector, Vector2 uv) vertB = (tri.b, vb, uvb);
            (int index, Vector3 vector, Vector2 uv) vertC = (tri.c, vc, uvc);

            if (!verts.Contains(vertA))
            {
                verts.Add(vertA);
            }
            if (!verts.Contains(vertB))
            {
                verts.Add(vertB);
            }
            if (!verts.Contains(vertC))
            {
                verts.Add(vertC);
            }

            // Check the direction of the triangle to ensure all are facing the same direction
            float a = tri.vb.x * tri.va.y + tri.vc.x * tri.vb.y + tri.va.x * tri.vc.y;
            float b = tri.va.x * tri.vb.y + tri.vb.x * tri.vc.y + tri.vc.x * tri.va.y;
            if (a > b)
            {
                indeces.Add(tri.a);
                indeces.Add(tri.b);
                indeces.Add(tri.c);
            }
            else
            {
                indeces.Add(tri.c);
                indeces.Add(tri.b);
                indeces.Add(tri.a);
            }
        }

        // Sort the array based on the index so that data is laid out appropriately
        verts.Sort((a, b) =>
        {
            return a.index < b.index ? -1 : 1;
        });

        Mesh mesh = new Mesh();
        mesh.vertices = verts.ConvertAll(_ => _.vector).ToArray();
        mesh.uv = verts.ConvertAll(_ => _.uv).ToArray(); ;
        mesh.triangles = indeces.ToArray();
        mesh.name = "generated_mesh";
        mesh.RecalculateNormals();

        MeshRenderer mr = ground.AddComponent<MeshRenderer>();
        mr.material = groundMaterial;
        MeshFilter mf = ground.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        return ground;
    }

    public GameObject GenerateStreets(List<StreetPropsData> streetProps, List<Vector2> streetPoints, Material streetMaterial, float streetWidth, bool generateBenches, bool generateLamps, float lampDistance, bool generateCars)
    {
        List<StreetPropsData> lamps = streetProps.FindAll(_ => _.type == StreetAssetType.Lamp);
        List<StreetPropsData> benches = streetProps.FindAll(_ => _.type == StreetAssetType.Bench);
        List<StreetPropsData> cars = streetProps.FindAll(_ => _.type == StreetAssetType.Car);

        GameObject streets = new GameObject();
        streets.name = "Streets";
        streets.transform.parent = regionGameObject.transform;

        List<GameObject> streetsList = new List<GameObject>();
        StreetGenerator generator = new StreetGenerator();

        float halfStreetWidth = streetWidth / 2f;

        for (int i = 0; i < streetPoints.Count; i++)
        {
            Vector2 start = streetPoints[i];
            Vector2 end = i == streetPoints.Count - 1 ? streetPoints[0] : streetPoints[i + 1];

            GameObject street = generator.GenerateStreet(start, end, streetMaterial, streetWidth);
            street.transform.parent = streets.transform;
            street.name = "street " + i;

            streetsList.Add(street);
        }

        if (generateLamps)
        {
            float zOffset = 1f - halfStreetWidth;
            float xOffset = halfStreetWidth - 2f;

            for (int i = 0; i < streetsList.Count; i++)
            {
                Vector2 start = streetPoints[i];
                Vector2 end = i == streetPoints.Count - 1 ? streetPoints[0] : streetPoints[i + 1];

                float distance = (end - start).magnitude;
                int blockCount = Mathf.FloorToInt(distance / lampDistance);

                for (int j = 0; j < blockCount; j++)
                {
                    GameObject streetLamp = GameObject.Instantiate(lamps[0].prefab);
                    streetLamp.transform.parent = streetsList[i].transform;
                    streetLamp.transform.localPosition = new Vector3(xOffset + lampDistance * j, 0f, zOffset);
                }
            }
        }

        if (generateBenches)
        {
            float zOffset = 0.1f - halfStreetWidth;
            float xOffset = halfStreetWidth + 1f;

            for (int i = 0; i < streetsList.Count; i++)
            {
                Vector2 start = streetPoints[i];
                Vector2 end = i == streetPoints.Count - 1 ? streetPoints[0] : streetPoints[i + 1];
                float distance = (end - start).magnitude - xOffset;

                if (distance < 20f) continue;

                // Start of street
                GameObject startBench = GameObject.Instantiate(benches[0].prefab);
                startBench.transform.parent = streetsList[i].transform;
                startBench.transform.localPosition = new Vector3(xOffset, 0f, zOffset);
                startBench.transform.localRotation = Quaternion.identity;

                // End of street
                GameObject endBench = GameObject.Instantiate(benches[0].prefab);
                endBench.transform.parent = streetsList[i].transform;
                endBench.transform.localPosition = new Vector3(distance, 0f, zOffset);
                endBench.transform.localRotation = Quaternion.identity;
            }
        }

        if(generateCars)
        {
            float zOffset = -1.5f;
            float xOffset = halfStreetWidth;

            for (int i = 0; i < streetsList.Count; i++)
            {
                Vector2 start = streetPoints[i];
                Vector2 end = i == streetPoints.Count - 1 ? streetPoints[0] : streetPoints[i + 1];
                float distance = (end - start).magnitude - xOffset;

                //int carCount = Mathf.FloorToInt(Random.RandomRange(0f, distance));
                GameObject car = GameObject.Instantiate(cars[0].prefab);
                car.transform.parent = streetsList[i].transform;
                car.transform.localPosition = new Vector3(Random.Range(0f, distance), 0f, zOffset);
                car.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);

                carGameObjects.Add(car);
            }
        }

        return streets;
    }

    public GameObject GeneratePowerTower(GameObject towerPrefab)
    {
        GameObject tower = GameObject.Instantiate(towerPrefab);
        tower.transform.parent = regionGameObject.transform;
        tower.transform.localPosition = new Vector3(voronoiRegion.centroid.x, 0f, voronoiRegion.centroid.y);
        return tower;
    }
}

public static class CitySectorFactory {
    public static CitySector CreateCitySector(VoronoiRegion region, BuildingGroupType groupType, string regionName)
    {
        CitySector sector = null;
        switch (groupType)
        {
            default:
                throw new KeyNotFoundException();
            case BuildingGroupType.Residential:
                sector = new ResidentialSector(region, regionName);
                break;
            case BuildingGroupType.Industrial:
                sector = new IndustrialSector(region, regionName);
                break;
            case BuildingGroupType.Park:
                sector = new ParkSector(region, regionName);
                break;
        }

        return sector;
    }

}

public class ResidentialSector : CitySector
{
    public ResidentialSector(VoronoiRegion region, string regionName) : base(region, regionName)
    {
        this.sectorType = BuildingGroupType.Residential;
    }

    public GameObject Generate(List<GameObject> streetsList, List<Vector2> streetPoints, float streetWidth, List<HouseComponentData> houseComponents, Material wireMaterial, List<ParkPropsData> parkPropsPrefabs, int minHeight, int maxHeight, int minWidth, int maxWidth)
    {
        GameObject residential = new GameObject("Residential");
        residential.transform.parent = regionGameObject.transform;

        for (int i = 0; i < streetsList.Count; i++)
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
            while (currentBlockCount < blockCount)
            {
                int height = Mathf.FloorToInt(Random.Range(minHeight, maxHeight + 1));
                int width = Mathf.FloorToInt(Random.Range(minWidth, maxWidth + 1));

                if (blockCount - currentBlockCount < minWidth)
                {
                    break;
                }

                if (blockCount - (currentBlockCount + width) < 0)
                {
                    width = blockCount - currentBlockCount;
                }

                GameObject house = houseGenerator.GenerateHouse(width, height, width == minWidth ? false : Random.value > 0.9f);
                house.name = "House " + houseCount++;
                house.transform.parent = streetsList[i].transform;
                house.transform.localPosition = GetHouseOffset(currentBlockCount, streetWidth);
                house.transform.localRotation = Quaternion.identity;
                houseGenerator.CreatePowerLine(house);

                currentBlockCount += width;
            }
        }

        List<ParkPropsData> treePrefabs = parkPropsPrefabs.FindAll(_ => _.type == ParkAssetType.Tree);
        List<ParkPropsData> fountainPrefabs = parkPropsPrefabs.FindAll(_ => _.type == ParkAssetType.Fountain);
        List<ParkPropsData> rockPrefabs = parkPropsPrefabs.FindAll(_ => _.type == ParkAssetType.Rock);

        // Points for the maximum extent the trees can spawn
        List<Vector2> treesExtent = UtilityFunctions.SortVectorsByAngle(streetPoints, voronoiRegion.boundsCenter);
        treesExtent = UtilityFunctions.InsetShape(treesExtent, 25f);

        // Generate points for the tree locations
        List<Vector2> treePositions = GenerateTreePoints(voronoiRegion.boundsCenter);
        treePositions = UtilityFunctions.FilterByShape(treesExtent, treePositions);
        treePositions = UtilityFunctions.FilterByRadius(treePositions, voronoiRegion.boundsCenter, 20f, true);

        GameObject trees = new GameObject();
        trees.name = "Trees";
        trees.transform.parent = residential.transform;

        int treeCount = 0;
        foreach (Vector2 position in treePositions)
        {
            GameObject tree = GameObject.Instantiate(treePrefabs[0].prefab);
            tree.name = "Tree " + treeCount++;
            tree.transform.parent = trees.transform;
            tree.transform.position = new Vector3(position.x, 0f, position.y);
            float randomScale = Random.Range(0.8f, 1.2f);
            tree.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        }

        return residential;
    }

    private Vector3 GetHouseOffset(int currentWidth, float streetWidth)
    {
        float houseNumberOffset = 2f * currentWidth;
        float halfStreet = streetWidth / 2f;
        return new Vector3(1f + streetWidth + houseNumberOffset, 0f, -halfStreet - 1f);
    }

    private List<Vector2> GenerateTreePoints(Vector2 center)
    {
        PoissonDisk disk = new PoissonDisk();
        List<Vector2> diskPoints = disk.GeneratePoints(100f, 5f, 10f);

        Vector2 diskPointsCenter = UtilityFunctions.FindBoundsCenter(diskPoints).center;

        for (int i = 0; i < diskPoints.Count; i++)
        {
            diskPoints[i] = diskPoints[i] - (diskPointsCenter - center);
        }

        return diskPoints;
    }
}

public class ParkSector : CitySector
{
    public ParkSector(VoronoiRegion region, string regionName) : base(region, regionName)
    {
        this.sectorType = BuildingGroupType.Park;
    }

    public GameObject Generate(bool generateFountain, List<Vector2> streetPoints, List<ParkPropsData> parkPropsPrefabs, Material groundMaterial, float insetAmount)
    {
        GameObject park = new GameObject("Park");
        park.transform.parent = regionGameObject.transform;

        List<ParkPropsData> treePrefabs = parkPropsPrefabs.FindAll(_ => _.type == ParkAssetType.Tree);
        List<ParkPropsData> fountainPrefabs = parkPropsPrefabs.FindAll(_ => _.type == ParkAssetType.Fountain);
        List<ParkPropsData> rockPrefabs = parkPropsPrefabs.FindAll(_ => _.type == ParkAssetType.Rock);
        List<ParkPropsData> specialPrefabs = parkPropsPrefabs.FindAll(_ => _.type == ParkAssetType.Special);

        List<(Vector2 mid, Vector2 connection)> connectionPoints = new List<(Vector2, Vector2)>();

        streetPoints = UtilityFunctions.SortVectorsByAngle(streetPoints, voronoiRegion.boundsCenter);
        List<Vector2> insetPoints = UtilityFunctions.InsetShape(streetPoints, insetAmount);
        Bounds bounds = UtilityFunctions.FindBoundsCenter(insetPoints);

        // TODO - Refactor in struct
        int subdivisionAmount = 2;
        float largestDiameter = UtilityFunctions.FindLargestDiameter(insetPoints, subdivisionAmount);
        bool isBigEnough = largestDiameter > 80f;
        bool generateGlassCastle = isBigEnough ? Random.value > 0.75f : false;

        (Vector2 start, Vector2 end) caps = UtilityFunctions.FindLargestDiameterEndPoints(insetPoints, subdivisionAmount);

        if (generateGlassCastle)
        {
            Vector2 midPoint = UtilityFunctions.GetMidpointOfLine(caps.start, caps.end);
            float largestDiameterAngle = UtilityFunctions.GetAngleDeg(caps.start, caps.end);

            GameObject castle = GameObject.Instantiate(specialPrefabs[0].prefab);
            castle.name = "Castle";
            castle.transform.parent = park.transform;
            castle.transform.localPosition = new Vector3(midPoint.x, 0f, midPoint.y - 6f);
            castle.transform.localRotation = Quaternion.Euler(0f, (largestDiameterAngle - 90f) + 180f, 0f);

            Vector2 delta = (caps.end - caps.start).normalized;
            delta *= 1.1f;

            StreetGenerator streetGenerator = new StreetGenerator();
            GameObject street = streetGenerator.GenerateStreet(caps.start, caps.end, groundMaterial, 10f);
            street.name = "Castle street";
            street.transform.parent = park.transform;
        } else
        {
            if (generateFountain)
            {
                GameObject fountain = GameObject.Instantiate(fountainPrefabs[0].prefab);
                fountain.transform.parent = park.transform;
                fountain.transform.localPosition = new Vector3(voronoiRegion.boundsCenter.x, 0f, voronoiRegion.boundsCenter.y);
                fountain.transform.localRotation = Quaternion.identity;

                GameObject fountainGround = new GameObject();
                fountainGround.transform.parent = fountain.transform;
                fountainGround.transform.localPosition = Vector3.zero;
                MeshRenderer mr = fountainGround.AddComponent<MeshRenderer>();
                mr.material = groundMaterial;
                MeshFilter mf = fountainGround.AddComponent<MeshFilter>();

                List<Vector2> circlePoints;

                mf.mesh = GenerateFountainMesh(6f, out circlePoints);

                for (int i = 0; i < circlePoints.Count; i++)
                {
                    circlePoints[i] += new Vector2(fountain.transform.position.x, fountain.transform.position.z);
                }

                circlePoints = UtilityFunctions.InsetShape(circlePoints, 2f);

                for (int i = 0; i < streetPoints.Count; i++)
                {
                    Vector2 start = streetPoints[i];
                    Vector2 end = i == streetPoints.Count - 1 ? streetPoints[0] : streetPoints[i + 1];

                    Vector2 fountainMidPoint = UtilityFunctions.GetMidpointOfLine(start, end);

                    connectionPoints.Add((fountainMidPoint, UtilityFunctions.GetClosestPoint(fountainMidPoint, circlePoints)));
                }

                for (int i = 0; i < connectionPoints.Count; i++)
                {
                    GameObject connection = new StreetGenerator().GenerateStreet(connectionPoints[i].mid, connectionPoints[i].connection, groundMaterial, 3f);
                    connection.name = "Fountain path " + i;
                    connection.transform.parent = fountain.transform;
                }
            }
        }

        // Generate points for the tree locations
        List<Vector2> treePositions = GenerateTreePoints(voronoiRegion.boundsCenter);
        treePositions = UtilityFunctions.FilterByShape(insetPoints, treePositions);

        if(generateGlassCastle)
        {
            treePositions = UtilityFunctions.FilterByLine(treePositions, caps.start, caps.end, 12.5f, true);
        } else if (generateFountain)
        {
            treePositions = UtilityFunctions.FilterByRadius(treePositions, voronoiRegion.boundsCenter, 10f, true);

            foreach ((Vector2 mid, Vector2 connection) connection in connectionPoints)
            {
                treePositions = UtilityFunctions.FilterByLine(treePositions, connection.mid, connection.connection, 3f, true);
            }
        }

        GameObject trees = new GameObject();
        trees.name = "Trees";
        trees.transform.parent = park.transform;

        int treeCount = 1;
        foreach (Vector2 position in treePositions)
        {
            GameObject tree = GameObject.Instantiate(treePrefabs[0].prefab);
            tree.name = "Tree " + treeCount++;
            tree.transform.parent = trees.transform;
            tree.transform.position = new Vector3(position.x, 0f, position.y);
            float randomScale = Random.Range(0.8f, 1.2f);
            tree.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        }

        return park;
    }

    private Mesh GenerateFountainMesh(float radius, out List<Vector2> circlPoints)
    {
        circlPoints = UtilityFunctions.GenerateCircleA(new Vector2(), radius, 24);

        DelaunayTriangulator.Triangulator triangulator = new DelaunayTriangulator.Triangulator();
        List<DelaunayTriangulator.Triad> triangles = triangulator.Triangulation(circlPoints.ConvertAll(_ => new DelaunayTriangulator.Vertex(_)), true);

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        // Find the lowest [x,y] and highest [x,y] coordinates
        foreach (DelaunayTriangulator.Triad tri in triangles)
        {
            if (tri.va.x < minX) minX = tri.va.x;
            if (tri.va.y < minY) minY = tri.va.y;
            if (tri.va.x > maxX) maxX = tri.va.x;
            if (tri.va.y > maxY) maxY = tri.va.y;

            if (tri.vb.x < minX) minX = tri.vb.x;
            if (tri.vb.y < minY) minY = tri.vb.y;
            if (tri.vb.x > maxX) maxX = tri.vb.x;
            if (tri.vb.y > maxY) maxY = tri.vb.y;

            if (tri.vc.x < minX) minX = tri.vc.x;
            if (tri.vc.y < minY) minY = tri.vc.y;
            if (tri.vc.x > maxX) maxX = tri.vc.x;
            if (tri.vc.y > maxY) maxY = tri.vc.y;
        }

        float rangeX = maxX - minX;
        float rangeY = maxY - minY;

        List<int> indeces = new List<int>();

        List<(int index, Vector3 vector, Vector2 uv)> verts = new List<(int index, Vector3 vector, Vector2 uv)>();

        foreach (DelaunayTriangulator.Triad tri in triangles)
        {
            Vector3 va = new Vector3(tri.va.x, 0f, tri.va.y);
            Vector3 vb = new Vector3(tri.vb.x, 0f, tri.vb.y);
            Vector3 vc = new Vector3(tri.vc.x, 0f, tri.vc.y);

            Vector2 uva = new Vector2((tri.va.x - minX) / rangeX, (tri.va.y - minY) / rangeY);
            Vector2 uvb = new Vector2((tri.vb.x - minX) / rangeX, (tri.vb.y - minY) / rangeY);
            Vector2 uvc = new Vector2((tri.vc.x - minX) / rangeX, (tri.vc.y - minY) / rangeY);

            (int index, Vector3 vector, Vector2 uv) vertA = (tri.a, va, uva);
            (int index, Vector3 vector, Vector2 uv) vertB = (tri.b, vb, uvb);
            (int index, Vector3 vector, Vector2 uv) vertC = (tri.c, vc, uvc);

            if (!verts.Contains(vertA))
            {
                verts.Add(vertA);
            }
            if (!verts.Contains(vertB))
            {
                verts.Add(vertB);
            }
            if (!verts.Contains(vertC))
            {
                verts.Add(vertC);
            }

            // Check the direction of the triangle to ensure all are facing the same direction
            float a = tri.vb.x * tri.va.y + tri.vc.x * tri.vb.y + tri.va.x * tri.vc.y;
            float b = tri.va.x * tri.vb.y + tri.vb.x * tri.vc.y + tri.vc.x * tri.va.y;
            if (a > b)
            {
                indeces.Add(tri.a);
                indeces.Add(tri.b);
                indeces.Add(tri.c);
            }
            else
            {
                indeces.Add(tri.c);
                indeces.Add(tri.b);
                indeces.Add(tri.a);
            }
        }

        // Sort the array based on the index so that data is laid out appropriately
        verts.Sort((a, b) =>
        {
            return a.index < b.index ? -1 : 1;
        });

        Mesh mesh = new Mesh();
        mesh.vertices = verts.ConvertAll(_ => _.vector).ToArray();
        mesh.uv = verts.ConvertAll(_ => _.uv).ToArray(); ;
        mesh.triangles = indeces.ToArray();
        mesh.name = "generated_mesh";
        mesh.RecalculateNormals();

        return mesh;
    }

    // TODO update to include optiosn for the poisson disk generation
    private List<Vector2> GenerateTreePoints(Vector2 center)
    {
        PoissonDisk disk = new PoissonDisk();
        List<Vector2> diskPoints = disk.GeneratePoints(100f, 5f, 10f);

        Vector2 diskPointsCenter = UtilityFunctions.FindBoundsCenter(diskPoints).center;

        for (int i = 0; i < diskPoints.Count; i++)
        {
            diskPoints[i] = diskPoints[i] - (diskPointsCenter - center);
        }

        return diskPoints;
    }
}

public class IndustrialSector : CitySector
{
    public IndustrialSector(VoronoiRegion region, string regionName) : base(region, regionName)
    {
        this.sectorType = BuildingGroupType.Industrial;
    }

    public GameObject Generate(List<GameObject> streetsList, List<Vector2> streetPoints, float streetWidth, List<FactoryComponentData> factoryComponentsData, List<IndustryPropsData> industryPropsData, float insetAmount, Material wireMaterial, int minWidth, int maxWidth)
    {
        GameObject industry = new GameObject("Industry");
        industry.transform.parent = regionGameObject.transform;

        List<IndustryPropsData> wallPrefabs = industryPropsData.FindAll(_ => _.type == IndustryAssetType.Wall);
        List<IndustryPropsData> wallEndPrefabs = industryPropsData.FindAll(_ => _.type == IndustryAssetType.WallEnd);
        List<IndustryPropsData> wallGatePrefabs = industryPropsData.FindAll(_ => _.type == IndustryAssetType.WallGate);

        List<Vector2> insetPoints = UtilityFunctions.InsetShape(streetPoints, insetAmount);

        float streetHalfWidth = streetWidth / 2f;

        Vector2 industry2D = new Vector2(industry.transform.position.x, industry.transform.position.z);

        GameObject walls = new GameObject("Walls");
        walls.transform.parent = industry.transform;
        walls.transform.localPosition = Vector3.zero;
        walls.transform.localRotation = Quaternion.identity;

        //int gateSide = Mathf.FloorToInt(Random.Range(0f, streetsList.Count - 1));

        // Generate walls and gates
        for (int i = 0; i < streetsList.Count; i++)
        {
            Vector2 start = insetPoints[i];
            Vector2 end = i == insetPoints.Count - 1 ? insetPoints[0] : insetPoints[i + 1];

            bool spawnGate = Vector2.Distance(start, end) > 30f;

            WallGenerator wallGenerator = new WallGenerator();

            GameObject wall = wallGenerator.GenerateWall(start, end, wallPrefabs[0].prefab, wallEndPrefabs[0].prefab, spawnGate, Random.value, wallGatePrefabs[0].prefab);
            wall.name = "Wall " + i;
            wall.transform.parent = walls.transform;
            wall.transform.localPosition = new Vector3(start.x, 0f, start.y);
        }



        int width = Mathf.FloorToInt(Random.Range(1f, 6f));
        // Generate factories

        int factoryCount = 0;
        for (int i = 0; i < streetsList.Count; i++)
        {
            Vector2 start = streetPoints[i];
            Vector2 end = i == streetPoints.Count - 1 ? streetPoints[0] : streetPoints[i + 1];

            float distance = (end - start).magnitude - (streetWidth * 2f);

            FactoryGenerator factoryGenerator = new FactoryGenerator();
            factoryGenerator.factoryComponents = factoryComponentsData;
            factoryGenerator.wireMaterial = wireMaterial;

            if (distance > 30f) {
                GameObject factory = factoryGenerator.GenerateFactory(minWidth, maxWidth);
                factory.name = "Factory " + i;
                factory.transform.parent = streetsList[i].transform;
                factory.transform.localPosition = new Vector3(distance / 3f + 12f, 0f, -12f);
                factory.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                factoryGenerator.CreatePowerLine(factory);

                factoryCount++;
            }
        }

        //if(factoryCount == 0)
        //{
        //    List<FactoryComponentData> resources = factoryComponentsData.FindAll(_ => _.type == FactoryComponentType.Resource);
        //    List<FactoryComponentData> storages = factoryComponentsData.FindAll(_ => _.type == FactoryComponentType.Storage);

        //    Vector3 zOffset = new Vector3(0f, 0f, -10f);

        //    for (int i = 0; i < streetsList.Count; i++)
        //    {
        //        Vector2 start = streetPoints[i];
        //        Vector2 end = i == streetPoints.Count - 1 ? streetPoints[0] : streetPoints[i + 1];

        //        float blockWidth = 2f;
        //        float distance = (end - start).magnitude - (streetWidth * 2f);
        //        int blockCount = Mathf.FloorToInt(distance / blockWidth / 12f);

        //        if (distance > 30f)
        //        {
        //            GameObject storage = GameObject.Instantiate(UtilityFunctions.GetWeightedRandom(new List<(float weight, GameObject gameObject)> {
        //                (0.5f, resources[0].prefab),
        //                (0.5f, storages[0].prefab)
        //            }));
        //            storage.name = "Storage " + i;
        //            storage.transform.parent = streetsList[i].transform;
        //            storage.transform.localPosition = new Vector3(0f, 0f, -12f) + GetStorageOffset(blockCount);
        //            storage.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        //        }
        //    }
        //}

        return industry;
    }

    private Vector3 GetStorageOffset(int storageCount)
    {
        float storageNumberOffset = 10f * storageCount;
        Vector3 offset = new Vector3(1f + storageNumberOffset, 0f, 0f);

        return offset;
    }
}