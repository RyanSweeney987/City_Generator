using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGenerator
{
    public GameObject GenerateWall(Vector2 start, Vector2 end, GameObject wallPrefab, GameObject wallEndPrefab, bool hasGate, float gateLocation, GameObject gatePrefab)
    {
        GameObject wall = new GameObject("Wall");
        
        float distance = Vector2.Distance(start, end);

        GameObject startWallCap = GameObject.Instantiate(wallEndPrefab);
        startWallCap.transform.parent = wall.transform;
        startWallCap.transform.localPosition = new Vector3(0f, 0f, -1f);
        startWallCap.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        
        GameObject endWallCap = GameObject.Instantiate(wallEndPrefab);
        endWallCap.transform.parent = wall.transform;
        endWallCap.transform.localPosition = new Vector3(0f, 0f, -distance + 1f);

        int wallCount = Mathf.FloorToInt(distance / 2f);
        float leftOver = distance % wallCount;
        int loopCount = leftOver > 0f ? wallCount - 1 : wallCount - 2;

        int gatePosition = -1;
        if (distance < 8f)
        {
            hasGate = false;
        } else
        {
            float lerpLoc = Mathf.Lerp(0f, distance - 6f, gateLocation);
            gatePosition = Mathf.FloorToInt(lerpLoc / 2f);
        }

        for(int i = 0; i < loopCount; i++)
        {
            float zOffset = (2f * i) + 3f;

            GameObject wallSegment = null;

            if (hasGate)
            {
                if(i == gatePosition)
                {
                    wallSegment = GameObject.Instantiate(gatePrefab);
                    wallSegment.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                }
                else if(i == gatePosition + 1)
                {
                    wallSegment = GameObject.Instantiate(gatePrefab);
                } else
                {
                    wallSegment = GameObject.Instantiate(wallPrefab);
                }
            } else
            {
                wallSegment = GameObject.Instantiate(wallPrefab);
            }

            wallSegment.transform.parent = wall.transform;
            wallSegment.transform.localPosition = new Vector3(0f, 0f, -zOffset);
        }

        float angle = UtilityFunctions.GetAngleDeg(end, start);
        wall.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        return wall;
    }
}
