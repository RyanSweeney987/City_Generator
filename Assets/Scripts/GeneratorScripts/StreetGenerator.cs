using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StreetGenerator
{
    public float width = 0.0f;
    public StreetType type = StreetType.Primary;

    // Generate street based on two points, start and end

    public GameObject GenerateStreet(Vector2 start, Vector2 end, Material material, float streetWidth = 0.1f)
    {
        // Order the points before generating the road mesh
        GameObject street = new GameObject();

        MeshRenderer mr = street.AddComponent<MeshRenderer>();
        mr.material = material;
        MeshFilter mf = street.AddComponent<MeshFilter>();

        mf.mesh = GenerateRoadMesh((end - start).magnitude, streetWidth);
        street.transform.position = new Vector3(start.x, 0f, start.y);

        float angleDeg = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        if(angleDeg > 90f)
        {
            angleDeg = 450f - angleDeg;
        } else
        {
            angleDeg = 90f - angleDeg;
        }
        angleDeg -= 90f;

        street.transform.rotation = Quaternion.Euler(0f, angleDeg, 0f);

        return street;
    }

    private Mesh GenerateRoadMesh(float distance, float streetWidth)
    {
        float halfWidth = streetWidth / 2f;

        Vector3 b = new Vector3(0f, 0f, 0f);
        Vector3 a = new Vector3(0f, 0f, -halfWidth);
        Vector3 c = new Vector3(distance, 0f, -halfWidth);
        Vector3 d = new Vector3(distance, 0f, 0f); ;

        Vector2 aUV = new Vector2(0f, 0f);
        Vector2 bUV = new Vector2(0f, 1f);
        Vector2 cUV = new Vector2(1f, 1f);
        Vector2 dUV = new Vector2(1f, 0f);

        int[] indeces = { 0, 1, 2, 2, 1, 3 };

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            a,
            b,
            c,
            d
        };
        mesh.uv = new Vector2[] {
            aUV,
            bUV,
            cUV,
            dUV
        };
 
        mesh.triangles = indeces;
        mesh.name = "generated_mesh";
        mesh.RecalculateNormals();

        return mesh;
    }

    //public static List<Vector2> SortPoints(List<Vector2> points, Vector2 center)
    //{
    //    List<(Vector2 p, float angle)> ps = new List<(Vector2 p, float angle)>();

    //    // Creates tuples that bind angle and the vector
    //    foreach (Vector2 point in points)
    //    {
    //        float angle = Mathf.Atan2(point.y - center.y, center.x - point.x) * Mathf.Rad2Deg;
    //        ps.Add((point, angle));
    //    }

    //    // Sort based on angle
    //    ps.Sort((a, b) =>
    //    {
    //        if (b.angle < a.angle)
    //        {
    //            return -1;
    //        }
    //        else if (b.angle > a.angle)
    //        {
    //            return 1;
    //        }
    //        else
    //        {
    //            return 0;
    //        }
    //    });

    //    return ps.ConvertAll(_ => _.p);
    //}

    //public static Vector2 FindBoundsCenter(List<Vector2> vertices)
    //{
    //    float minX = 100000f;
    //    float minY = 100000f;
    //    float maxX = -100000f;
    //    float maxY = -100000f;

    //    foreach (Vector2 point in vertices)
    //    {
    //        if (point.x < minX)
    //        {
    //            minX = point.x;
    //        }
    //        if (point.x > maxX)
    //        {
    //            maxX = point.x;
    //        }
    //        if (point.y < minY)
    //        {
    //            minY = point.y;
    //        }
    //        if (point.y > maxY)
    //        {
    //            maxY = point.y;
    //        }
    //    }

    //    return new Vector2(minX, minY) + (new Vector2(maxX - minX, maxY - minY) / 2f);
    //}
}

public enum StreetType
{
    Primary,
    Secondary,
    Alley
}
