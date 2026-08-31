using UnityEngine;

public class GJK : MonoBehaviour
{
    [HideInInspector] public Simplex simplex = new Simplex();
    public SupportPoint Support(ConvexCollider colliderA, ConvexCollider colliderB, Vector3 direction)
    {
        Vector3 pointA = colliderA.FindFurtherPoint(direction);
        Vector3 pointB = colliderB.FindFurtherPoint(-direction);

        return new SupportPoint
        {
            support = pointA - pointB,
            pointA = pointA,
            pointB = pointB
        };
    }

    public bool SameDirection(Vector3 direction, Vector3 ao)
    {
        return Vector3.Dot(direction.normalized, ao.normalized) > 0;
    }   

    bool Line(Simplex simplex, ref Vector3 direction)
    {
        SupportPoint a = simplex.points[0];
        SupportPoint b = simplex.points[1];

        Vector3 ab = b.support - a.support;
        Vector3 ao = -a.support;

        if (SameDirection(ab, ao))
        {
            direction = Vector3.Cross(Vector3.Cross(ab, ao), ab);
        }
        else
        {
            simplex.SetPoint(a);
            direction = ao; 
        }

        return false;
    }

    bool Triangle(Simplex simplex, ref Vector3 direction) 
    {
        SupportPoint a = simplex.points[0];
        SupportPoint b = simplex.points[1];
        SupportPoint c = simplex.points[2];

        Vector3 ab = b.support - a.support;
        Vector3 ac = c.support - a.support;
        Vector3 ao = -a.support;

        Vector3 abc = Vector3.Cross(ab, ac);

        if (SameDirection(Vector3.Cross(abc, ac), ao)) 
        {
            if (SameDirection(ac, ao))
            {
                simplex.SetLine(a, c);
                direction = Vector3.Cross(Vector3.Cross(ac, ao), ac);
            }
            else
            {
                simplex.SetLine(a, b);
                return Line(simplex, ref direction);
            }
        }
        else
        {
            if(SameDirection(Vector3.Cross(ab, abc), ao))
            {
                simplex.SetLine(a, b);
                return Line(simplex, ref direction); 
            }
            else
            {
                if(SameDirection(abc, ao))
                {
                    direction = abc; 
                }
                else
                {
                    simplex.SetTriangle(a, b, c);
                    direction = -abc; 
                }
            }
        }

        return false; 
    }

    bool Tetrahedron(Simplex simplex, ref Vector3 direction)
    {
        SupportPoint a = simplex.points[0];
        SupportPoint b = simplex.points[1];
        SupportPoint c = simplex.points[2];
        SupportPoint d = simplex.points[3];

        Vector3 ab = b.support - a.support;
        Vector3 ac = c.support - a.support;
        Vector3 ad = d.support - a.support;
        Vector3 ao = -a.support;

        Vector3 abc = Vector3.Cross(ab, ac); 
        Vector3 acd = Vector3.Cross(ac, ad);
        Vector3 adb = Vector3.Cross(ad, ab);

        if (SameDirection(abc, ao))
        {
            simplex.SetTriangle(a, b, c);
            return Triangle(simplex, ref direction); 
        }

        if (SameDirection(acd, ao))
        {
            simplex.SetTriangle(a, c, d);
            return Triangle(simplex, ref direction); 
        }

        if (SameDirection(adb, ao))
        {
            simplex.SetTriangle(a, d, b);
            return Triangle(simplex, ref direction); 
        }

        return true; 
    }

    bool HandleSimplex(Simplex simplex, ref Vector3 direction)
    {
        switch (simplex.GetSize())
        {
            case 2:
                return Line(simplex, ref direction);
            case 3:
                return Triangle(simplex, ref direction);
            case 4:
                return Tetrahedron(simplex, ref direction); 
        }

        return false; 
    }

    public bool NarrowPhase(ConvexCollider colliderA, ConvexCollider colliderB)
    {
        SupportPoint points = Support(colliderA, colliderB, new Vector3(1f, 1f, 1f));
        simplex = new Simplex();
        simplex.PushFront(points);

        Vector3 direction = -points.support;

        int maxIterations = 50; 
        int iterations = 0;

        while (true)
        {
            iterations++;
            if (iterations > maxIterations)
            {
                Debug.LogWarning("GJK a dépassé le nombre maximal d'itérations (boucle infinie évitée).");
                return false; 
            }


            points = Support(colliderA, colliderB, direction);

            if (Vector3.Dot(points.support, direction) <= 0)
            {
                return false;
            }

            simplex.PushFront(points);

            if (HandleSimplex(simplex, ref direction))
            {
                return true;
            }
        }
    }
}