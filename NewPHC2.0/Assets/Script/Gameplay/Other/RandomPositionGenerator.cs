using UnityEngine;

public class RandomPositionGenerator
{
    public static Vector3 GetRandomPositionInCollider(Collider2D collider)
    {
        if (collider is BoxCollider2D boxCollider)
        {
            return GetRandomPositionInBoxCollider(boxCollider);
        }
        else if (collider is CircleCollider2D circleCollider)
        {
            return GetRandomPositionInCircleCollider(circleCollider);
        }
        else if (collider is PolygonCollider2D polygonCollider)
        {
            return GetRandomPositionInPolygonCollider(polygonCollider);
        }
        else if (collider is CapsuleCollider2D capsuleCollider)
        {
            return GetRandomPositionInCapsuleCollider(capsuleCollider);
        }
        else if (collider is EdgeCollider2D edgeCollider)
        {
            return GetRandomPositionInEdgeCollider(edgeCollider);
        }
        else
        {
            return Vector2.zero;
        }
    }

    private static Vector2 GetRandomPositionInBoxCollider(BoxCollider2D boxCollider)
    {
        Vector2 center = boxCollider.bounds.center;
        Vector2 size = boxCollider.bounds.size;
        return new Vector2(
            Random.Range(center.x - size.x / 2, center.x + size.x / 2),
            Random.Range(center.y - size.y / 2, center.y + size.y / 2)
        );
    }

    private static Vector2 GetRandomPositionInCircleCollider(CircleCollider2D circleCollider)
    {
        Vector2 center = circleCollider.bounds.center;
        float radius = circleCollider.radius;
        Vector2 randomPoint = Random.insideUnitCircle * radius;
        return center + randomPoint;
    }

    private static Vector2 GetRandomPositionInPolygonCollider(PolygonCollider2D polygonCollider)
    {
        // Get the points of the polygon
        Vector2[] points = polygonCollider.points;
        // Transform points to world space
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = polygonCollider.transform.TransformPoint(points[i]);
        }

        // Create a bounding box around the polygon
        Bounds bounds = polygonCollider.bounds;

        Vector2 randomPoint;
        do
        {
            randomPoint = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );
        } while (!polygonCollider.OverlapPoint(randomPoint)); // Ensure the point is inside the polygon

        return randomPoint;
    }

    private static Vector2 GetRandomPositionInCapsuleCollider(CapsuleCollider2D capsuleCollider)
    {
        Vector2 center = capsuleCollider.bounds.center;
        Vector2 size = capsuleCollider.size;
        float height = size.y;
        float radius = size.x / 2;

        // Randomly choose between the two ends of the capsule or the area in between
        if (Random.value < 0.5f)
        {
            // Choose one of the ends
            return new Vector2(
                Random.Range(center.x - radius, center.x + radius),
                Random.Range(center.y - height / 2, center.y + height / 2)
            );
        }
        else
        {
            // Choose a point in the middle area
            return new Vector2(
                Random.Range(center.x - radius, center.x + radius),
                Random.Range(center.y - height / 2 + radius, center.y + height / 2 - radius)
            );
        }
    }

    private static Vector2 GetRandomPositionInEdgeCollider(EdgeCollider2D edgeCollider)
    {
        Vector2[] points = edgeCollider.points;
        if (points.Length < 2)
        {
            Debug.LogWarning("EdgeCollider2D must have at least 2 points.");
            return Vector2.zero;
        }

        // Randomly select two points and interpolate between them
        int index1 = Random.Range(0, points.Length);
        int index2 = Random.Range(0, points.Length);
        Vector2 point1 = edgeCollider.transform.TransformPoint(points[index1]);
        Vector2 point2 = edgeCollider.transform.TransformPoint(points[index2]);

        return Vector2.Lerp(point1, point2, Random.value);
    }
}