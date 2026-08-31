using UnityEngine;
using System.Collections.Generic;

public enum shapeType 
{
    SPHERE,
    CUBE,
    NONE,
}

public class ConvexCollider : MonoBehaviour
{
    private Vector3[] vertices;
    private Mesh mesh;

    public shapeType shape = shapeType.NONE; 

    public void Awake()
    {
        if (GetComponent<MeshFilter>().mesh != null)
        {
            mesh = GetComponent<MeshFilter>().mesh;
            vertices = mesh.vertices;
        }
    }

    public Vector3 FindFurtherPoint(Vector3 direction)
    {
        Vector3 localDir = transform.InverseTransformDirection(direction);

        float maxDot = float.NegativeInfinity;
        Vector3 furthestPoint = Vector3.zero;

        foreach (var vertex in vertices)
        {
            float dot = Vector3.Dot(vertex, localDir);
            if (dot > maxDot)
            {
                maxDot = dot;
                furthestPoint = vertex;
            }
        }

        return transform.TransformPoint(furthestPoint);
    }
}
