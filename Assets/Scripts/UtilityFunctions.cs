using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityFunctions
{
    public static Vector2 GetIntersectPoint(Vector2 startA, Vector2 endA, Vector2 startB, Vector2 endB)
    {
        float a1 = endA.y - startA.y;
        float b1 = startA.x - endA.x;
        float c1 = a1 * startA.x + b1 * startA.y;

        float a2 = endB.y - startB.y;
        float b2 = startB.x - endB.x;
        float c2 = a2 * startB.x + b2 * startB.y;

        float det = a1 * b2 - a2 * b1;

        if (det == 0)
        {
            return new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        } else {
            float x = (b2 * c1 - b1 * c2) / det;
            float y = (a1 * c2 - a2 * c1) / det;
            return new Vector2(x, y);
        }
    }

    public static float GetAngleDeg(Vector2 start, Vector2 end)
    {
        float angleDeg = GetAngle(start, end) * Mathf.Rad2Deg;

        if (angleDeg > 90f)
        {
            angleDeg = 450f - angleDeg;
        }
        else
        {
            angleDeg = 90f - angleDeg;
        }

        return angleDeg;
    }

    public static float GetAngle(Vector2 start, Vector2 end)
    {
        return Mathf.Atan2(end.y - start.y, end.x - start.x);
    }
    public static T GetWeightedRandom<T>(List<(float weight, T item)> options)
    {
        float total = 0f;
        foreach (float weight in options.ConvertAll(_ => _.weight))
        {
            total += weight;
        }

        float random = Random.value / (1f / total);

        foreach ((float weight, T item) choice in options)
        {
            if (random <= choice.weight)
            {
                return choice.item;
            }

            random -= choice.weight;
        }

        // return the first option by default
        return options[0].item;
    }

    public static List<Vector2> GenerateCircleA(Vector2 midPoint, float radius, int vertCount)
    {
        float r = radius * (Mathf.PI / 2.0f);
        // Get the average distance for each point around the circle
        float arcLength = radius * (1f / vertCount) * (2.0f * Mathf.PI / radius);
        // Get the number of points based on the length
        int pointCount = Mathf.CeilToInt(2.0f * Mathf.PI / arcLength);
        // Create the points
        List<Vector2> points = new List<Vector2>();
        float theta = 0f;
        for (int i = 0; i < pointCount; i++)
        {
            theta += arcLength;
            float x = r * Mathf.Cos(theta) + midPoint.x;
            float y = r * Mathf.Sin(theta) + midPoint.y;
            points.Add(new Vector2(x, y));
        }

        return points;
    }

    public static List<Vector2> GenerateCircle(Vector2 midPoint, float radius)
    {
        float r = radius * (Mathf.PI / 2.0f) * 1.25f;
        // Get the average distance for each point around the circle
        float arcLength = radius * 0.1f * (2.0f * Mathf.PI / radius);
        // Get the number of points based on the length
        int pointCount = Mathf.CeilToInt(2.0f * Mathf.PI / arcLength);
        // Create the points
        List<Vector2> points = new List<Vector2>();
        float theta = 0f;
        for (int i = 0; i < pointCount; i++)
        {
            theta += arcLength;
            float x = r * Mathf.Cos(theta) + midPoint.x;
            float y = r * Mathf.Sin(theta) + midPoint.y;
            points.Add(new Vector2(x, y));
        }

        return points;
    }

    public static Vector2 GetNormal(Vector2 a, Vector2 b, bool getRightNormal = true)
    {
        // https://stackoverflow.com/questions/1243614/how-do-i-calculate-the-normal-vector-of-a-line-segment
        Vector2 tangent = a - b;
        Vector2 normal = getRightNormal ? new Vector2(-tangent.y, tangent.x) : new Vector2(tangent.y, -tangent.x);
        return normal.normalized;
    }

    public static Bounds FindBoundsCenter(List<Vector2> points)
    {
        Bounds bounds = new Bounds();

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (Vector2 point in points)
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

        bounds.SetMinMax(new Vector3(minX, minY), new Vector3(maxX, maxY));

        return bounds;
    }

    public static Vector2 GetMidpointOfLine(Vector2 start, Vector2 end)
    {
        return (start + end) / 2f;
    }

    public static Vector2 GetClosestPoint(Vector2 start, List<Vector2> points)
    {
        Vector2 closestPoint = start;
        float distance = float.MaxValue;

        foreach(Vector2 point in points)
        {
            float mag = (point - start).magnitude;
            if (mag < distance)
            {
                distance = mag;
                closestPoint = point;
            }
        }

        return closestPoint;
    }

    public static List<Vector2> SortVectorsByAngle(List<Vector2> points, Vector2 center, bool sortAntiClockwise = true)
    {
        List<(Vector2 p, float angle)> ps = new List<(Vector2 p, float angle)>();

        // Creates tuples that bind angle and the vector
        foreach (Vector2 point in points)
        {
            float angle = Mathf.Atan2(point.y - center.y, center.x - point.x) * Mathf.Rad2Deg;
            ps.Add((point, angle));
        }

        // Sort based on angle
        ps.Sort((a, b) =>
        {
            if (b.angle < a.angle)
            {
                return sortAntiClockwise ? -1 : 1;
            }
            else if (b.angle > a.angle)
            {
                return sortAntiClockwise ? 1 : -1;
            }
            else
            {
                return 0;
            }
        });

        return ps.ConvertAll(_ => _.p);
    }

    public static List<Vector2> InsetShape(List<Vector2> points, float insetValue)
    {
        List<Vector2> result = new List<Vector2>();

        for (int i = 0; i < points.Count; i++)
        {
            int afterIndex = (i == points.Count - 1 ? 0 : i + 1);
            int beforeIndex = (i == 0 ? points.Count - 1 : i - 1);

            Vector2 nA = GetNormal(points[afterIndex], points[i]);
            Vector2 nB = GetNormal(points[i], points[beforeIndex]);

            Vector2 normal = (nA + nB).normalized;

            result.Add(points[i] + (normal * insetValue));
        }

        return result;
    }

    public static List<Vector2> FilterByShape(List<Vector2> shape, List<Vector2> points, bool inverted = false)
    {
        List<Vector2> result = new List<Vector2>();

        DelaunayTriangulator.Triangulator triangulator = new DelaunayTriangulator.Triangulator();
        List<DelaunayTriangulator.Triad> triangles = triangulator.Triangulation(shape.ConvertAll(_ => new DelaunayTriangulator.Vertex(_)), true);

        foreach (DelaunayTriangulator.Triad tri in triangles)
        {
            foreach (Vector2 point in points)
            {
                if (IsInTriangle(point, tri.va, tri.vb, tri.vc))
                {
                    result.Add(point);
                }
            }
        }

        return result;
    }

    public static List<Vector2> FilterByRadius(List<Vector2> points, Vector2 center, float radius, bool invert = false, bool isRect = false)
    {
        List<Vector2> result = new List<Vector2>();

        if (isRect)
        {
            Vector2 min = new Vector2(center.x - radius, center.y - radius);
            Vector2 max = new Vector2(center.x + radius, center.y + radius);

            foreach (Vector2 point in points)
            {
                bool isInside = point.x > min.x && point.y > min.y && point.x < max.x && point.y < max.y;
                if (invert ? !isInside : isInside)
                {
                    result.Add(point);
                }
            }
        }
        else
        {
            foreach (Vector2 point in points)
            {
                bool isInside = (center - point).magnitude < radius;
                if (invert ? !isInside : isInside)
                {
                    result.Add(point);
                }
            }
        }

        return result;
    }

    public static List<Vector2> FilterByLine(List<Vector2> points, Vector2 start, Vector2 end, float width, bool invert = false)
    {
        List<Vector2> result = new List<Vector2>();

        foreach(Vector2 point in points)
        {
            float distance = ((end.x - start.x) * (start.y - point.y) - (start.x - point.x) * (end.y - start.y)) / Vector2.Distance(start, end);
            bool isInside = Mathf.Abs(distance) < width;

            if(invert ? !isInside : isInside)
            {
                result.Add(point);
            }
        }

        return result;
    }

    // Found this via
    // https://stackoverflow.com/a/14382692
    // With a support JSfiddle
    // http://jsfiddle.net/PerroAZUL/zdaY8/1/
    // Uses barycentric coordinates assuming anti-clockwise direction (I think), I don't really understand it
    // but it's apparently better than an alternative method which is the half plane method
    public static bool IsInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float A = 1f / 2f * (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y);
        float sign = A < 0f ? -1f : 1f;
        float s = (p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y) * sign;
        float t = (p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y) * sign;

        return s > 0f && t > 0f && (s + t) < 2f * A * sign;
    }

    public static List<Vector2> Subdivide(List<Vector2> points, int subdivisions, int current = 0)
    {
        List<Vector2> newPoints = new List<Vector2>(points);

        if (current >= subdivisions)
        {
            return newPoints;
        } else
        {
            List<Vector2> midPoints = new List<Vector2>();

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 start = points[i];
                Vector2 end = i == newPoints.Count - 1 ? newPoints[0] : newPoints[i + 1];

                midPoints.Add(GetMidpointOfLine(start, end));
            }

            newPoints.AddRange(midPoints);
            newPoints = SortVectorsByAngle(newPoints, FindBoundsCenter(newPoints).center);

            current++;

            return Subdivide(newPoints, subdivisions, current);
        }
    }

    public static float FindLargetDiameterAngle(List<Vector2> points, int subdivisions = 0)
    {
        List<Vector2> subPoints = Subdivide(points, subdivisions);

        float distance = float.MinValue;
        float angle = 0f;

        foreach(Vector2 point in subPoints)
        {
            foreach(Vector2 point2 in subPoints)
            {
                if (point == point2) continue;

                float newDistance = Vector2.Distance(point, point2);
                if (newDistance > distance)
                {
                    distance = newDistance;
                    angle = GetAngleDeg(point, point2);
                }
            }
        }

        return angle;
    }

    public static float FindLargestDiameter(List<Vector2> points, int subdivisions = 0)
    {
        List<Vector2> subPoints = Subdivide(points, subdivisions);

        float distance = float.MinValue;

        foreach (Vector2 point in subPoints)
        {
            foreach (Vector2 point2 in subPoints)
            {
                if (point == point2) continue;

                float newDistance = Vector2.Distance(point, point2);
                if (newDistance > distance)
                {
                    distance = newDistance;
                }
            }
        }

        return distance;
    }

    public static Vector2 FindLargestDiameterMidpoint(List<Vector2> points, int subdivisions = 0)
    {
        List<Vector2> subPoints = Subdivide(points, subdivisions);

        float distance = float.MinValue;
        Vector2 midPoint = Vector2.zero;

        foreach (Vector2 point in subPoints)
        {
            foreach (Vector2 point2 in subPoints)
            {
                if (point == point2) continue;

                float newDistance = Vector2.Distance(point, point2);
                if (newDistance > distance)
                {
                    distance = newDistance;
                    midPoint = GetMidpointOfLine(point, point2);
                }
            }
        }

        return midPoint;
    }

    public static (Vector2 start, Vector2 end) FindLargestDiameterEndPoints(List<Vector2> points, int subdivisions = 0)
    {
        List<Vector2> subPoints = Subdivide(points, subdivisions);

        float distance = float.MinValue;
        Vector2 start = Vector2.zero;
        Vector2 end = Vector2.zero;

        foreach (Vector2 point in subPoints)
        {
            foreach (Vector2 point2 in subPoints)
            {
                if (point == point2) continue;

                float newDistance = Vector2.Distance(point, point2);
                if (newDistance > distance)
                {
                    distance = newDistance;
                    start = point;
                    end = point2;
                }
            }
        }

        return (start, end);
    }

    public static Vector2 RandomVector2(float minInclusive, float maxInclusive)
    {
        return new Vector2(Random.Range(minInclusive, maxInclusive), Random.Range(minInclusive, maxInclusive));
    }
}