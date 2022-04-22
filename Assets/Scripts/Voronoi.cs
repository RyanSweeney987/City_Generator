using DelaunayTriangulator;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiGraph
{
    public List<DelaunayTriangle> triangles;
    public List<VoronoiRegion> regions;
    public List<Vector2> points;

    public VoronoiGraph()
    {
        triangles = new List<DelaunayTriangle>();
        regions = new List<VoronoiRegion>();
    }

    public VoronoiGraph(List<Vector2> vertices) : this()
    {
        points = vertices;
        // Find bounds to generate surrounding circle
        Bounds bounds = UtilityFunctions.FindBoundsCenter(points);
        float radius = (bounds.center - bounds.min).magnitude;
        // Generate surrounding circle
        List<Vector2> edgeVerts = UtilityFunctions.GenerateCircle(bounds.center, radius);

        List<Vector2> finalList = new List<Vector2>(points);
        finalList.AddRange(edgeVerts);
        // Convert to vertex for use with the Triangulator (3rd party)
        List<Vertex> verts = finalList.ConvertAll(_ => new Vertex(_));

        Triangulator triangulator = new Triangulator();
        List<Triad> triads = triangulator.Triangulation(verts, true);

        List<DelaunayTriangle> nodes = new List<DelaunayTriangle>();
        // Adding all triangles to nodes
        foreach (Triad triangle in triads)
        {
            DelaunayTriangle node = new DelaunayTriangle(triangle);
            nodes.Add(node);
        }

        // Loop through the nodes and add neighbours
        for (int i = 0; i < triads.Count; i++)
        {
            if (triads[i].ab != -1)
            {
                nodes[i].neighbours.Add(nodes[triads[i].ab]);
            }
            if (triads[i].ac != -1)
            {
                nodes[i].neighbours.Add(nodes[triads[i].ac]);
            }
            if (triads[i].bc != -1)
            {
                nodes[i].neighbours.Add(nodes[triads[i].bc]);
            }
        }

        List<VoronoiRegion> voronoiRegions = new List<VoronoiRegion>();
        foreach (Vector2 point in points)
        {
            List<DelaunayTriangle> siteVerts = nodes.FindAll(_ => _.triangle.va == point || _.triangle.vb == point || _.triangle.vc == point);

            regions.Add(new VoronoiRegion(siteVerts, point));
        }
    }

    public VoronoiGraph GenerateVoronoiGraph(float graphSize, float minRadius, float maxRadius, Vector2 offset)
    {
        // Generate the points 
        PoissonDisk pd = new PoissonDisk();

        this.points = new List<Vector2>();

        while(this.points.Count < 3)
        {
            this.points.Clear();
            this.points.AddRange(pd.GeneratePoints(graphSize, minRadius, maxRadius, 10));
        }
        
        for (int i = 0; i < points.Count; i++)
        {
            points[i] += offset;
        }

        return new VoronoiGraph(points);
    }
}

public class DelaunayTriangle
{
    public Triad triangle;
    public List<DelaunayTriangle> neighbours;

    public DelaunayTriangle(Triad triangle)
    {
        this.triangle = triangle;
        this.neighbours = new List<DelaunayTriangle>();
    }

    public override bool Equals(object obj)
    {
        return obj is DelaunayTriangle vertex &&
               EqualityComparer<Triad>.Default.Equals(triangle, vertex.triangle) &&
               EqualityComparer<List<DelaunayTriangle>>.Default.Equals(neighbours, vertex.neighbours);
    }

    public override int GetHashCode()
    {
        int hashCode = -996946507;
        hashCode = hashCode * -1521134295 + EqualityComparer<Triad>.Default.GetHashCode(triangle);
        hashCode = hashCode * -1521134295 + EqualityComparer<List<DelaunayTriangle>>.Default.GetHashCode(neighbours);
        return hashCode;
    }

    public static bool operator ==(DelaunayTriangle c1, DelaunayTriangle c2)
    {
        return c1.Equals(c2);
    }

    public static bool operator !=(DelaunayTriangle c1, DelaunayTriangle c2)
    {
        return !c1.Equals(c2);
    }

    public void DrawTriangle()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        Gizmos.DrawLine(new Vector3(triangle.va.x, 0.0f, triangle.va.y), new Vector3(triangle.vb.x, 0.0f, triangle.vb.y));
        Gizmos.DrawLine(new Vector3(triangle.vb.x, 0.0f, triangle.vb.y), new Vector3(triangle.vc.x, 0.0f, triangle.vc.y));
        Gizmos.DrawLine(new Vector3(triangle.vc.x, 0.0f, triangle.vc.y), new Vector3(triangle.va.x, 0.0f, triangle.va.y));
    }

    public void DrawCircumCenter()
    {
        Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 1.0f);
        Gizmos.DrawWireSphere(new Vector3(triangle.circumCenter.x, 0.0f, triangle.circumCenter.y), 1f);
    }
}

public class VoronoiRegion
{
    public Vector2 centroid = Vector2.zero;
    // Center of triangles that make up the vertices
    public Vector2 siteVertex = Vector2.zero; 
    public Vector2 boundsCenter = Vector2.zero;
    public List<Triad> regionTriangles = new List<Triad>();
    // Triangles that contain the circumcenter that is used in the edge
    public List<DelaunayTriangle> triangles = new List<DelaunayTriangle>(); 
    public List<Vector2> edgePoints = new List<Vector2>();

    public VoronoiRegion(List<DelaunayTriangle> triangles, Vector2 siteVertex)
    {
        this.triangles = triangles;
        this.siteVertex = siteVertex;
        centroid = FindCircumCircle(triangles);
        boundsCenter = FindBoundsCenter(triangles);
        edgePoints = GetEdgePoints(triangles);
    }

    private Vector2 FindCircumCircle(List<DelaunayTriangle> vertices)
    {
        List<Vertex> verts = vertices.ConvertAll(_ => new Vertex(_.triangle.circumCenter));

        Vector2 totalCircumCenter = Vector2.zero;
        float totalArea = 0.0f;

        regionTriangles.AddRange(new Triangulator().Triangulation(verts, true));

        foreach (Triad tri in regionTriangles)
        {
            Vector2 av = new Vector2(verts[tri.a].x, verts[tri.a].y);
            Vector2 bv = new Vector2(verts[tri.b].x, verts[tri.b].y);
            Vector2 cv = new Vector2(verts[tri.c].x, verts[tri.c].y);

            float abM = (av - bv).magnitude;
            float bcM = (bv - cv).magnitude;
            float acM = (av - cv).magnitude;

            float area = 0.25f * Mathf.Sqrt((abM + bcM + acM) * (-abM + bcM + acM) * (abM - bcM + acM) * (acM + bcM - acM));
            Vector2 currentCentroid = new Vector2(tri.circumcircleX, tri.circumcircleY) * area;

            totalArea += area;
            totalCircumCenter += currentCentroid;
        }

        return totalCircumCenter / totalArea;
    }

    private Vector2 FindBoundsCenter(List<DelaunayTriangle> vertices)
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (Vector2 point in vertices.ConvertAll(_ => _.triangle.circumCenter))
        {
            if (point.x < minX)
            {
                minX = point.x;
            }
            if (point.x > maxX)
            {
                maxX = point.x;
            }
            if (point.y < minY)
            {
                minY = point.y;
            }
            if (point.y > maxY)
            {
                maxY = point.y;
            }
        }

        return new Vector2(minX, minY) + (new Vector2(maxX - minX, maxY - minY) / 2f);
    }

    private List<Vector2> GetEdgePoints(List<DelaunayTriangle> triangles)
    {
        List<Vector2> points = triangles.ConvertAll(_ => _.triangle.circumCenter);

        points = UtilityFunctions.SortVectorsByAngle(points, boundsCenter);

        return points;
    }

    public void DrawRegionTriangles()
    {
        foreach(DelaunayTriangle triangle in triangles)
        {
            triangle.DrawTriangle();
        }
    }

    public void DrawRegionTriangleCircumCenters()
    {
        foreach (DelaunayTriangle triangle in triangles)
        {
            triangle.DrawCircumCenter();
        }
    }
    
    public void DrawRegionCircumCenter()
    {
        Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 1.0f);
        Gizmos.DrawWireSphere(new Vector3(centroid.x, 0.0f, centroid.y), 1f);
    }
}