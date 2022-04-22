using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundGenerator
{
    public Mesh GenerateGroundMesh(List<DelaunayTriangulator.Triad> triangles)
    {
        float minX = 100000f;
        float minY = 100000f;
        float maxX = -100000f;
        float maxY = -100000f;

        // Find the lowest [x,y] and highest [x,y] coordinates
        foreach(DelaunayTriangulator.Triad tri in triangles)
        {
            if(tri.va.x < minX) minX = tri.va.x;
            if(tri.va.y < minY) minY = tri.va.y;
            if(tri.va.x > maxX) maxX = tri.va.x;
            if(tri.va.y > maxY) maxY = tri.va.y;

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
}
