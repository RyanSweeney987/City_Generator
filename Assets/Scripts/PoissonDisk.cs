using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf
public class PoissonDisk
{
    private const float SQRT2 = 1.41421356237f;

    public List<Vector2> GeneratePoints(float size, float minRadius, float maxRadius = -1f, int sampleCount = 10) {
        // Default max radius if not specified
        if(maxRadius == -1f) maxRadius = minRadius;

        // Cell size for the background grid
        float cellSize = minRadius / SQRT2;
        int cellSideCount = Mathf.CeilToInt(size / cellSize);

        // Background grid to speed up checking
        int[] backgroundGrid = new int[cellSideCount * cellSideCount];
        for (int i = 0; i < backgroundGrid.Length; i++) {
            backgroundGrid[i] = -1;
        }

        // Create random starting vector
        Vector2 randVector = UtilityFunctions.RandomVector2(0f, size);
        // Add this vector to the final points list
        List<Vector2> pointsList = new List<Vector2>();
        pointsList.Add(randVector);
        // Get the linear vector index for the grid position of the initial vector
        int sampledCellIndex = GetSampleIndex(randVector, cellSize, cellSideCount);
        backgroundGrid[sampledCellIndex] = pointsList.Count - 1;
        // Active list of grid positions to find potential new vectors around
        List<int> activeList = new List<int>();
        activeList.Add(sampledCellIndex);

        // Keep looping until the active list becomes empty
        // Active list will be empty when no more candidates can be found
        while(activeList.Count > 0) {
            // Pick a random sample from the active list and get the vector
            int activeListIndex = Mathf.FloorToInt(Random.Range(0f, activeList.Count));
            int gridSampleIndex = activeList[activeListIndex];
            int pointsListIndex = backgroundGrid[gridSampleIndex];
            Vector2 sourcePoint = pointsList[pointsListIndex];

            bool noCandidates = true;

            for (int i = 0; i < sampleCount; i++)
            {
                Vector2 candidate = GetCandidatePosition(sourcePoint, minRadius, maxRadius);

                // Check to make sure that the candidate is within the bounds
                if (candidate.x >= 0 && candidate.x <= size && candidate.y >= 0 && candidate.y <= size)
                {
                    // Get all the indeces of the neighbouring tiles
                    List<int> neighbourCellIndeces = GetNeighbouringCellIndeces(candidate, cellSize, cellSideCount);

                    // Loop through to find any potential neighbouring points
                    List<Vector2> neighbouringPoints = new List<Vector2>();
                    foreach (int index in neighbourCellIndeces)
                    {
                        int gridIndex = backgroundGrid[index];
                        if (gridIndex >= 0) {
                            neighbouringPoints.Add(pointsList[gridIndex]);
                        }
                    }

                    // Check if the candidate point is valid against the neighbours (if any)
                    if(IsValidLocation(neighbouringPoints, candidate, minRadius))
                    {
                        int candidateSampleIndex = GetSampleIndex(candidate, cellSize, cellSideCount);

                        // If it is, add it to the points list and to the active list
                        pointsList.Add(candidate);
                        backgroundGrid[candidateSampleIndex] = pointsList.Count - 1;
                        activeList.Add(candidateSampleIndex);

                        noCandidates = false;

                        break;
                    }
                }
            }

            // If there's no potential candidates, remove it from the active list
            if (noCandidates) activeList.RemoveAt(activeListIndex);
        }

        return pointsList;
    }

    public List<Vector2> GeneratePointsOriginal(float size, float minRadius, float maxRadius = -1f, int sampleCount = 10)
    {
        // Default max radius if not specified
        if (maxRadius == -1f)
        {
            maxRadius = minRadius;
        }

        // Cell size for the background grid
        float cellSize = minRadius / SQRT2;
        int cellSideCount = Mathf.CeilToInt(size / cellSize);

        // Background grid to speed up checking
        int[] backgroundGrid = new int[cellSideCount * cellSideCount];
        for (int i = 0; i < backgroundGrid.Length; i++)
        {
            backgroundGrid[i] = -1;
        }

        // Create random starting vector
        Vector2 randVector = UtilityFunctions.RandomVector2(0f, size);
        // Add this vector to the final points list
        List<Vector2> pointsList = new List<Vector2>();
        pointsList.Add(randVector);
        // Get the linear vector index for the grid position of the initial vector
        int sampledCellIndex = GetSampleIndex(randVector, cellSize, cellSideCount);
        backgroundGrid[sampledCellIndex] = pointsList.Count - 1;
        // Active list of grid positions to find potential new vectors around
        List<int> activeList = new List<int>();
        activeList.Add(sampledCellIndex);

        int loopCount = 0;

        while (activeList.Count > 0)
        {
            // Pick a random sample from the active list and get the vector
            int activeListIndex = Mathf.FloorToInt(Random.Range(0f, activeList.Count));
            int gridSampleIndex = activeList[activeListIndex];
            int pointsListIndex = backgroundGrid[gridSampleIndex];
            Vector2 sourcePoint = pointsList[pointsListIndex];

            bool noCandidates = true;

            for (int i = 0; i < sampleCount; i++)
            {
                Vector2 candidate = GetCandidatePosition(sourcePoint, minRadius, maxRadius);

                // Check to make sure that the candidate is within the bounds
                if (candidate.x >= 0 && candidate.x <= size && candidate.y >= 0 && candidate.y <= size)
                {
                    // Get all the indeces of the neighbouring tiles
                    List<int> neighbourCellIndeces = GetNeighbouringCellIndeces(candidate, cellSize, cellSideCount);

                    // Loop through to find any potential neighbouring points
                    List<Vector2> neighbouringPoints = new List<Vector2>();
                    foreach (int index in neighbourCellIndeces)
                    {
                        if (index < 0 || index >= backgroundGrid.Length)
                        {
                            Debug.Log("Neighbour index: " + index);
                        }

                        int gridIndex = backgroundGrid[index];
                        if (gridIndex >= 0)
                        {
                            neighbouringPoints.Add(pointsList[gridIndex]);
                        }
                    }

                    // Check if the candidate point is valid against the neighbours (if any)
                    if (IsValidLocation(neighbouringPoints, candidate, minRadius))
                    {
                        int candidateSampleIndex = GetSampleIndex(candidate, cellSize, cellSideCount);

                        if (candidateSampleIndex >= backgroundGrid.Length)
                        {
                            Debug.Log("Sample index is greater than background grid capacity");
                            continue;
                        }

                        // If it is, add it to the points list and to the active list
                        pointsList.Add(candidate);
                        backgroundGrid[candidateSampleIndex] = pointsList.Count - 1;
                        activeList.Add(candidateSampleIndex);

                        noCandidates = false;

                        break;
                    }
                }
            }

            // If there's no potential candidates, remove it from the active list
            if (noCandidates)
            {
                activeList.RemoveAt(activeListIndex);
            }

            // Quick solution to an unsolved bug
            loopCount++;
            if (loopCount > backgroundGrid.Length * sampleCount)
            {
                Debug.Log("Encountered infinite loop, exiting loop. Active list count: " + activeList.Count);
                break;
            }
        }

        // Post processing to fix errors
        List<Vector2> invalidPoints = new List<Vector2>();

        foreach (Vector2 pointA in pointsList)
        {
            foreach (Vector2 pointB in pointsList)
            {
                if (pointA == pointB)
                {
                    continue;
                }
                else if (Vector2.Distance(pointA, pointB) < minRadius)
                {
                    if (!invalidPoints.Contains(pointA))
                    {
                        invalidPoints.Add(pointA);
                    }
                }
            }
        }

        if (invalidPoints.Count > 0)
        {
            Debug.Log(invalidPoints.Count + " invalid points");
        }

        return pointsList;
    }

    private int GetSampleIndex(Vector2 vector, float cellSize, int cellSideCount)
    {
        int xIndex = Mathf.FloorToInt(vector.x / cellSize);
        int yIndex = Mathf.FloorToInt(vector.y / cellSize);
        return yIndex * cellSideCount + xIndex;
    }

    private Vector2 GetCandidatePosition(Vector2 sampledVector, float minRadius, float maxRadius)
    {
        float angle = Random.value * Mathf.PI * 2.0f;
        float magnitude = Random.Range(minRadius, maxRadius);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * magnitude + sampledVector;
    }

    private List<int> GetNeighbouringCellIndeces(Vector2 vector, float cellSize, int cellSideCount)
    {
        int xIndex = Mathf.FloorToInt(vector.x / cellSize);
        int yIndex = Mathf.FloorToInt(vector.y / cellSize);

        int searchStartX = Mathf.Max(0, xIndex - 2);
        int searchEndX = Mathf.Min(xIndex + 2, cellSideCount - 1);
        int searchStartY = Mathf.Max(0, yIndex - 2);
        int searchEndY = Mathf.Min(yIndex + 2, cellSideCount - 1);

        List<int> neighbours = new List<int>();
        for (int y = searchStartY; y <= searchEndY; y++)
        {
            for (int x = searchStartX; x <= searchEndX; x++)
            {
                neighbours.Add(y * cellSideCount + x);
            }
        }

        return neighbours;
    }

    private bool IsValidLocation(List<Vector2> neighbours, Vector2 candidate, float minRadius)
    {
        foreach(Vector2 neighbour in neighbours)
        {
            if (Vector2.Distance(candidate, neighbour) < minRadius)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsValidLocation(int[] grid, List<Vector2> points, Vector2 candidate, float minRadius, float cellSize, int cellSideCount)
    {
        int cellX = Mathf.FloorToInt(candidate.x / cellSize);
        int cellY = Mathf.FloorToInt(candidate.y / cellSize);
        int searchStartX = Mathf.Max(0, cellX - 2);
        int searchEndX = Mathf.Min(cellX + 2, cellSideCount);
        int searchStartY = Mathf.Max(0, cellY - 2);
        int searchEndY = Mathf.Min(cellY + 2, cellSideCount);

        for(int y = searchStartY; y < searchEndY; y++)
        {
            for(int x = searchStartX; x < searchEndX; x++)
            {
                int gridIndex = y * cellSideCount + x;
                int pointsIndex = grid[gridIndex];
                Vector2 neighbour = points[pointsIndex];

                if ((candidate - neighbour).magnitude < minRadius)
                {
                    return false;
                }
            }
        }

        return true;
    }
}