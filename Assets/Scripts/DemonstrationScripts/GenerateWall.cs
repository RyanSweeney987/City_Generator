using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWall : MonoBehaviour
{
    public Vector2 start;
    public Vector2 end;
    public GameObject wallPrefab;
    public GameObject wallEndPrefab;
    public GameObject wallColumnPrefab;

    [Header("Gate Settings")]
    public bool hasGate = false;
    [Range(0f, 1f)]
    public float location = 0.5f;
    public bool randomise = true;
    public GameObject gatePrefab;

    private GameObject wall;

    public void Generate()
    {
        this.Clear();

        WallGenerator wallGenerator = new WallGenerator();
        if(randomise)
        {
            wall = wallGenerator.GenerateWall(start,end, wallPrefab, wallEndPrefab, Random.value > 0.5f, Random.value, gatePrefab);
        } else { 
            wall = wallGenerator.GenerateWall(start, end, wallPrefab, wallEndPrefab, hasGate, location, gatePrefab);
        }

        Vector2 offset = start - UtilityFunctions.GetMidpointOfLine(start, end);
        wall.transform.position = new Vector3(offset.x, 0f, offset.y);
    }

    public void Clear()
    {
        if (wall != null && wall.activeInHierarchy)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(wall);
            }
            else if (Application.isPlaying)
            {
                Destroy(wall);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(start.x, 0f, start.y), new Vector3(end.x, 0f, end.y));
    }
}
