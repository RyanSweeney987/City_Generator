using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePiossonDisk : MonoBehaviour
{
    [Header("Settings")]
    public float graphSize = 10f;
    public float pointRadius = 1f;
    public float pointMaxRadius = 2f;
    public bool useMaxRadius = false;
    public Vector2 offset;

    [Header("Debug Settings")]
    public float sphereRadius = 1f;

    private List<Vector2> points = new List<Vector2>();
    public void Generate()
    {
        // Generate the points 
        PoissonDisk pd = new PoissonDisk();
        this.points = pd.GeneratePoints(graphSize, pointRadius, pointMaxRadius, 10);

        for (int i = 0; i < points.Count; i++)
        {
            points[i] += offset;
        }
    }

    public void Clear()
    {
        if(points != null && points.Count > 0)
        {
            points.Clear();
        }
    }

    private void OnDrawGizmos()
    {
       
        if(points != null && points.Count > 0)
        {
            foreach(Vector2 point in points)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(new Vector3(point.x, 0f, point.y), 0.25f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(new Vector3(point.x, 0f, point.y), pointRadius);
            }
        }
    }
}
