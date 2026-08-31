using UnityEngine;

public class AABB
{
    public Vector3 min;
    public Vector3 max;

    public AABB(Vector3 _min, Vector3 _max)
    {
        min = _min;
        max = _max;
    }

    public bool CheckOverlap(AABB other)
    {
        return (min.x <= other.max.x && max.x >= other.min.x) &&
               (min.y <= other.max.y && max.y >= other.min.y) &&
               (min.z <= other.max.z && max.z >= other.min.z);
    }

    public bool Contains(AABB other)
    {
        return (min.x <= other.min.x && max.x >= other.max.x) &&
               (min.y <= other.min.y && max.y >= other.max.y) &&
               (min.z <= other.min.z && max.z >= other.max.z);
    }

    public static AABB Union(AABB a, AABB b)
    {
        Vector3 min = new Vector3(
        Mathf.Min(a.min.x, b.min.x),
        Mathf.Min(a.min.y, b.min.y),
        Mathf.Min(a.min.z, b.min.z)
    );

        Vector3 max = new Vector3(
            Mathf.Max(a.max.x, b.max.x),
            Mathf.Max(a.max.y, b.max.y),
            Mathf.Max(a.max.z, b.max.z)
        );

        return new AABB(min, max);
    }

    public float Volume()
    {
        Vector3 size = max - min;
        return size.x * size.y * size.z;
    }

    public AABB Expanded(float margin)
    {
        Vector3 m = new Vector3(margin, margin, margin);
        return new AABB(min - m, max + m);
    }
}