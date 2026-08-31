using UnityEngine;

public static class InertiaMoment
{
    public static Matrix4x4 Calculate(shapeType shape, float mass, Bounds bounds)
    {
        switch (shape)
        {
            case shapeType.CUBE:
                Vector3 size = bounds.size;
                float ix = (1f / 12f) * mass * (size.y * size.y + size.z * size.z);
                float iy = (1f / 12f) * mass * (size.x * size.x + size.z * size.z);
                float iz = (1f / 12f) * mass * (size.x * size.x + size.y * size.y);
                Matrix4x4 tensor = Matrix4x4.identity;
                tensor[0, 0] = ix;
                tensor[1, 1] = iy;
                tensor[2, 2] = iz;
                return tensor;
            case shapeType.SPHERE:
                float r = bounds.extents.x;
                float i = (2f / 5f) * mass * r * r;
                Matrix4x4 tensorSphere = Matrix4x4.identity;
                tensorSphere[0, 0] = i;
                tensorSphere[1, 1] = i;
                tensorSphere[2, 2] = i;
                return tensorSphere;
            default:
                Debug.LogWarning("Shape type not recognized for inertia tensor calculation");
                return Matrix4x4.identity;
        }
    }
}
