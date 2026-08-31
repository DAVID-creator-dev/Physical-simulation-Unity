using System.Collections.Generic;
using UnityEngine;

public struct SupportPoint
{
    public Vector3 support;
    public Vector3 pointA;
    public Vector3 pointB;
}

public class Simplex
{
    public List<SupportPoint> points = new List<SupportPoint>();

    public int GetSize() => points.Count;

    public void PushFront(SupportPoint point)
    {
        points.Insert(0, point);

        if (points.Count > 4)
            points.RemoveAt(points.Count - 1);
    }

    public void SetPoint(SupportPoint a)
    {
        points.Clear();
        points.Add(a);
    }   

    public void SetLine(SupportPoint a, SupportPoint b)
    {
        points.Clear();  
        points.Add(a);
        points.Add(b);
    }
    public void SetTriangle(SupportPoint a, SupportPoint b, SupportPoint c)
    {
        points.Clear();
        points.Add(a);
        points.Add(b);
        points.Add(c);
    }

    public void SetTetrahedron(SupportPoint a, SupportPoint b, SupportPoint c, SupportPoint d)
    {
        points.Clear();
        points.Add(a);
        points.Add(b);
        points.Add(c);
        points.Add(d);
    }

    public bool CheckSimplex()
    {
        if (points.Count != 4)
        {
            Debug.LogWarning($"Simplex invalide pour EPA : {points.Count} points trouvés (attendu : 4).");
            return false;
        }

        Vector3 ab = points[1].support - points[0].support;
        Vector3 ac = points[2].support - points[0].support;
        Vector3 ad = points[3].support - points[0].support;
        float volume = Mathf.Abs(Vector3.Dot(Vector3.Cross(ab, ac), ad));
        if (volume < 1e-6f)
        {
            Debug.LogWarning("Simplex invalide : les points sont coplanaires.");
            return false;
        }

        return true;
    }
}
