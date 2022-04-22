using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePark : MonoBehaviour
{
    [Header("Street Generation")]
    public List<Vector2> streetPoints;
    public float streetWidth = 1f;
    public Material streetMaterial;
    public bool connectLoop = false;
    public Vector2 pointsOffset;

    [Header("Lamp Generation")]
    public bool generateLamps = false;
    public float lampDistance = 5f;
    public GameObject lampPrefab;

    [Header("Bench Generation")]
    public bool generateBenches = false;
    public GameObject benchPrefab;

    [Header("Fountain Generation")]
    public bool generateFountain = false;
    public bool randomiseFountainGeneration = true;
    public Vector2 fountainLocation;
    public Material groundMaterial;

    [Header("Park Generation")]
    public float insetAmount;
    public GameObject treePrefab;
    public GameObject parkBenchPrefab;
    public GameObject fountainPrefab;
    public GameObject rockPrefab1;
    public GameObject rockPrefab2;

    private GameObject streets;
    private List<Vector2> treePositions;

    private List<DelaunayTriangulator.Triad> TempTris;

    public void Generate()
    {
        Clear();

        streets = new GameObject();
        streets.name = "Park Streets";

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

        if (generateLamps)
        {
            for (int i = 0; i < streetsList.Count; i++)
            {
                Vector2 start = streetPoints[i];
                Vector2 end = i == streetPoints.Count - 1 ? streetPoints[0] : streetPoints[i + 1];

                float distance = (end - start).magnitude;
                int blockCount = Mathf.FloorToInt(distance / lampDistance);

                for (int j = 0; j < blockCount; j++)
                {
                    GameObject streetLamp = Instantiate(lampPrefab);
                    streetLamp.transform.parent = streetsList[i].transform;
                    streetLamp.transform.localPosition = new Vector3(lampDistance * j, 0f, 0f);
                }
            }
        }

        if (generateBenches)
        {
            float zOffset = 0.1f - halfStreetWidth;
            float xOffset = 1f + halfStreetWidth;

            for (int i = 0; i < streetsList.Count; i++)
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

        Vector2 center = UtilityFunctions.FindBoundsCenter(streetPoints).center;

        List<(Vector2 mid, Vector2 connection)> connectionPoints = new List<(Vector2, Vector2)>();

        generateFountain = randomiseFountainGeneration ? Random.value > 0.5f : generateFountain;

        if (generateFountain)
        {
            GameObject fountain = Instantiate(fountainPrefab);
            fountain.transform.parent = streets.transform;
            fountain.transform.localPosition = new Vector3(center.x + fountainLocation.x, 0f, center.y + fountainLocation.y);
            fountain.transform.localRotation = Quaternion.identity;

            GameObject fountainGround = new GameObject();
            fountainGround.transform.parent = fountain.transform;
            fountainGround.transform.localPosition = Vector3.zero;
            MeshRenderer mr = fountainGround.AddComponent<MeshRenderer>();
            mr.material = groundMaterial;
            MeshFilter mf = fountainGround.AddComponent<MeshFilter>();

            List<Vector2> circlePoints;

            mf.mesh = GenerateFountainMesh(6f, out circlePoints);

            for(int i = 0; i < circlePoints.Count; i++)
            {
                circlePoints[i] += new Vector2(fountain.transform.position.x, fountain.transform.position.z);
            }

            circlePoints = UtilityFunctions.InsetShape(circlePoints, 2f);

            for(int i = 0; i < streetPoints.Count; i++)
            {
                Vector2 start = streetPoints[i];
                Vector2 end = i == streetPoints.Count - 1 ? streetPoints[0] : streetPoints[i + 1];

                Vector2 midPoint = UtilityFunctions.GetMidpointOfLine(start, end);

                connectionPoints.Add((midPoint, UtilityFunctions.GetClosestPoint(midPoint, circlePoints)));
            }

            for(int i = 0; i < connectionPoints.Count; i++)
            {
                GameObject connection = new StreetGenerator().GenerateStreet(connectionPoints[i].mid, connectionPoints[i].connection, groundMaterial, 3f);
                connection.name = "Fountain path " + (i + 1);
                connection.transform.parent = fountain.transform;
            }
        }

        // Points for the maximum extent the trees can spawn
        List<Vector2> treesExtent = UtilityFunctions.SortVectorsByAngle(streetPoints, center);
        treesExtent = UtilityFunctions.InsetShape(treesExtent, insetAmount);

        // Generate points for the tree locations
        treePositions = GenerateTreePoints(center);
        treePositions = UtilityFunctions.FilterByShape(treesExtent, treePositions);

        if (generateFountain)
        {
            treePositions = UtilityFunctions.FilterByRadius(treePositions, center + fountainLocation, 10f, true);
            
            foreach((Vector2 mid, Vector2 connection) connection in connectionPoints)
            {
                treePositions = UtilityFunctions.FilterByLine(treePositions, connection.mid, connection.connection, 3f, true);
            }
        }

        GameObject trees = new GameObject();
        trees.name = "Trees";
        trees.transform.parent = streets.transform;

        int treeCount = 1;
        foreach(Vector2 position in treePositions)
        {
            GameObject tree = Instantiate(treePrefab);
            tree.name = "Tree " + treeCount++;
            tree.transform.parent = trees.transform;
            tree.transform.position = new Vector3(position.x, 0f, position.y);
            float randomScale = Random.Range(0.8f, 1.2f);
            tree.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        }

    }

    // TODO update to include optiosn for the poisson disk generation
    private List<Vector2> GenerateTreePoints(Vector2 center)
    {
        PoissonDisk disk = new PoissonDisk();
        List<Vector2> diskPoints = disk.GeneratePoints(100f, 5f, 10);

        Vector2 diskPointsCenter = UtilityFunctions.FindBoundsCenter(diskPoints).center;

        for(int i = 0; i < diskPoints.Count; i++)
        {
            diskPoints[i] = diskPoints[i] - (diskPointsCenter - center);
        }

        return diskPoints;
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

    public void Clear()
    {
        if (streets != null && streets.activeInHierarchy)
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

        if(treePositions != null) {
            treePositions.Clear();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if(streetPoints != null && streetPoints.Count > 0) {
            List<Vector3> points = streetPoints.ConvertAll(_ => new Vector3(_.x, 0f, _.y));
            for (int i = 0; i < points.Count - 1; i++)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
        }

        Gizmos.color = Color.white;
        if(treePositions != null && treePositions.Count > 0)
        {
            foreach(Vector2 point in treePositions)
            {
                Gizmos.DrawWireSphere(new Vector3(point.x, 0f, point.y), 1f);
            }
        }

        Gizmos.color = Color.red;
        if(TempTris != null && TempTris.Count > 0) {
            foreach(DelaunayTriangulator.Triad triangle in TempTris) {
                Gizmos.DrawLine(new Vector3(triangle.va.x, 0f, triangle.va.y), new Vector3(triangle.vb.x, 0f, triangle.vb.y));
                Gizmos.DrawLine(new Vector3(triangle.vb.x, 0f, triangle.vb.y), new Vector3(triangle.vc.x, 0f, triangle.vc.y));
                Gizmos.DrawLine(new Vector3(triangle.vc.x, 0f, triangle.vc.y), new Vector3(triangle.va.x, 0f, triangle.va.y));
            }
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(new Vector3(fountainLocation.x, 0f, fountainLocation.y), 1f);
    }
}
