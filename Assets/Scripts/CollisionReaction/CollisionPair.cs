using System.Collections.Generic;
using Unity.Mathematics;
using Unity.XR.OpenVR;
using UnityEngine;

public class CollisionPair
{
    public Rigidbody A;
    public Rigidbody B;

    public float penetration;
    public Vector3 normal;
    public bool hasCollision = false;
    public Vector3 contactPointA;
    public Vector3 contactPointB; 

    public CollisionPair(Rigidbody a, Rigidbody b)
    {
        A = a;
        B = b;
    }

    public void CollisionReply(GJK gjk)
    {
        EPA(gjk);

        // Positional correction
        if (penetration > 0.0001f && normal != Vector3.zero)
        {
            float damping = 0.2f;
            float correction = (penetration * damping) / ((1f / A.m) + (1f / B.m));

            if (!A.isStatic && !B.isStatic)
            {
                A.transform.position -= (1f / A.m) * correction * normal;
                B.transform.position += (1f / B.m) * correction * normal;
            }
            else if (!A.isStatic)
            {
                A.transform.position -= normal * penetration;
            }
            else if (!B.isStatic)
            {
                B.transform.position += normal * penetration;
            }
        }


        // Rotational and Linear Impulse Calculation
        float weightRotA = 0, weightRotB = 0;
        Vector3 vAi = Vector3.zero, vBi = Vector3.zero;
        Vector3 momentumA = Vector3.zero, momentumB = Vector3.zero;
        Vector3 rA = Vector3.zero; 
        Vector3 rB = Vector3.zero;   

        if (!A.isStatic )
        {
            rA = contactPointA - A.transform.position;
            vAi = A.linearVelocity + Vector3.Cross(A.angularVelocity, rA);

            Matrix4x4 R_A = Matrix4x4.Rotate(A.transform.rotation);
            Matrix4x4 InA_world = R_A * A.inertiaTensor * R_A.transpose;

            momentumA = InA_world.inverse.MultiplyVector(Vector3.Cross(rA, normal));
            weightRotA = Vector3.Dot(Vector3.Cross(momentumA, rA), normal); 
        }

        if (!B.isStatic)
        {
            rB = contactPointB - B.transform.position;
            vBi = B.linearVelocity + Vector3.Cross(B.angularVelocity, rB);

            Matrix4x4 R_B = Matrix4x4.Rotate(B.transform.rotation);
            Matrix4x4 InB_world = R_B * B.inertiaTensor * R_B.transpose;

            momentumB = InB_world.inverse.MultiplyVector(Vector3.Cross(rB, normal));
            weightRotB = Vector3.Dot(Vector3.Cross(momentumB, rB), normal);
        }

        float vRel = Vector3.Dot(vAi - vBi, normal);

        float Ja = (-(1 + A.restitution) * vRel) / (A.GetInvMass() + B.GetInvMass() + weightRotA + weightRotB);
        float Jb = (-(1 + B.restitution) * vRel) / (A.GetInvMass() + B.GetInvMass() + weightRotA + weightRotB);

        if (!A.isStatic)
        {
            A.linearVelocity += Ja * A.GetInvMass() * normal;
            A.angularVelocity += Ja * momentumA;
        }

        if (!B.isStatic)
        {
            B.linearVelocity -= Jb * B.GetInvMass() * normal;
            B.angularVelocity -= Jb * momentumB;
        }

        float J = 0.5f * (Mathf.Abs(Ja) + Mathf.Abs(Jb));
        ApplyFriction(J, rA, rB, vAi - vBi);
    }

    void ApplyFriction(float J, Vector3 rA, Vector3 rB, Vector3 relVel)
    {
        Vector3 tangent = relVel - Vector3.Dot(relVel, normal) * normal;
        Vector3 t; 

        if (tangent.magnitude > 0f)
            t = tangent / tangent.magnitude;
        else
            t = Vector3.zero;

        Vector3 relTa = Vector3.Cross(t, rA);
        Vector3 relTb = Vector3.Cross(t, rB);

        Vector3 momentumA = A.inertiaTensor.inverse * relTa;
        Vector3 momentumB = B.inertiaTensor.inverse * relTb;

        float lambda = A.GetInvMass() + B.GetInvMass() + Vector3.Dot(relTa, momentumA) + Vector3.Dot(relTb, momentumB);

        float Jt = -Vector3.Dot(relVel, t) / lambda;

        float staticFriction = (A.staticFriction + B.staticFriction) / 2f;

        if (Jt <= staticFriction * J)
        {
            Jt = -Vector3.Dot(relVel, t) / lambda;
        }
        else
        {
            float kineticFriction = (A.dynamicFriction + B.dynamicFriction) / 2f;
            Jt = kineticFriction * J; 
        }

        if (!A.isStatic) 
        {
            A.linearVelocity += Jt * A.GetInvMass() * t;
            A.angularVelocity += Vector3.Cross(rA, Jt * t);

            A.linearVelocity *= (1f - A.linearDamping * Time.deltaTime);
            A.angularVelocity *= 1f / (1f + A.angularDamping * Time.deltaTime);
        }
        if (!B.isStatic) 
        {
            B.linearVelocity -= Jt * B.GetInvMass() * t;
            B.angularVelocity -= Vector3.Cross(rB, Jt * t);

            B.linearVelocity *= (1f - B.linearDamping * Time.deltaTime);
            B.angularVelocity *= 1f / (1f + B.angularDamping * Time.deltaTime);
        }
    }

    public void EPA(GJK gjk)
    {
        if (!gjk.simplex.CheckSimplex())
            return;

        List<SupportPoint> polytope = new List<SupportPoint>(gjk.simplex.points);

        List<int> faces = new List<int> {
        0, 1, 2,
        0, 3, 1,
        0, 2, 3,
        1, 3, 2
        };

        var (normals, minFace) = GetFaceNormals(polytope, faces);
        Vector3 minNormal = new Vector3();
        float minDistance = float.MaxValue;

        int maxIterations = 50;
        int iteration = 0;

        while (iteration < maxIterations)
        {
            iteration++;

            minNormal = normals[minFace];
            minDistance = normals[minFace].w;

            SupportPoint points = gjk.Support(A.collider, B.collider, minNormal);

            float sDistance = Vector3.Dot(minNormal, points.support);

            if (Mathf.Abs(sDistance - minDistance) < 0.001f)
            {
                normal = minNormal;
                penetration = minDistance + 0.001f;
                hasCollision = true;

                int fi = minFace * 3;
                SupportPoint a = polytope[faces[fi]];
                SupportPoint b = polytope[faces[fi + 1]];
                SupportPoint c = polytope[faces[fi + 2]];

                Vector3 n = minNormal.normalized;
                float d = minDistance;

                Vector3 proj = n * d;

                Vector3 ab = b.support - a.support;
                Vector3 ac = c.support - a.support;
                Vector3 ap = proj - a.support;

                float d00 = Vector3.Dot(ab, ab);
                float d01 = Vector3.Dot(ab, ac);
                float d11 = Vector3.Dot(ac, ac);
                float d20 = Vector3.Dot(ap, ab);
                float d21 = Vector3.Dot(ap, ac);
                float denom = d00 * d11 - d01 * d01;

                float v = (d11 * d20 - d01 * d21) / denom;
                float w = (d00 * d21 - d01 * d20) / denom;
                float u = 1.0f - v - w;

                contactPointA = u * a.pointA + v * b.pointA + w * c.pointA;
                contactPointB = u * a.pointB + v * b.pointB + w * c.pointB;

                Vector3 contactPoint = 0.5f * (contactPointA + contactPointB);

                // --- DEBUG ---
                Debug.DrawRay(contactPoint, normal.normalized * 0.5f, Color.red, 2.0f);
                Debug.DrawRay(contactPoint, -normal.normalized * 0.5f, Color.blue, 2.0f);

                Debug.DrawLine(contactPoint + Vector3.up * 0.05f, contactPoint - Vector3.up * 0.05f, Color.yellow, 2.0f);
                Debug.DrawLine(contactPoint + Vector3.right * 0.05f, contactPoint - Vector3.right * 0.05f, Color.yellow, 2.0f);
                Debug.DrawLine(contactPoint + Vector3.forward * 0.05f, contactPoint - Vector3.forward * 0.05f, Color.yellow, 2.0f);
                // --------------

                break;
            }

            List<(int, int)> uniqueEdges = new List<(int, int)>();
            List<int> visibleFaces = new List<int>();

            for (int i = 0; i < normals.Count; i++)
            {
                float dot = Vector3.Dot(normals[i], points.support - polytope[faces[i * 3]].support);
                if (dot > 0f)
                    visibleFaces.Add(i);
            }

            foreach (int i in visibleFaces)
            {
                int f = i * 3;
                AddIfUniqueEdge(uniqueEdges, faces, f, f + 1);
                AddIfUniqueEdge(uniqueEdges, faces, f + 1, f + 2);
                AddIfUniqueEdge(uniqueEdges, faces, f + 2, f);
            }

            visibleFaces.Sort((a, b) => b.CompareTo(a));
            foreach (int i in visibleFaces)
            {
                int f = i * 3;
                faces.RemoveRange(f, 3);
                normals.RemoveAt(i);
            }

            SupportPoint supportPoint = new SupportPoint
            {
                support = points.support,
                pointA = points.pointA,
                pointB = points.pointB
            };  

            polytope.Add(supportPoint);

            List<int> newFaces = new List<int>();
            foreach (var (a, b) in uniqueEdges)
            {
                newFaces.Add(a);
                newFaces.Add(b);
                newFaces.Add(polytope.Count - 1);
            }

            var (newNormals, _) = GetFaceNormals(polytope, newFaces);

            faces.AddRange(newFaces);
            normals.AddRange(newNormals);

            minFace = 0;
            minDistance = normals[0].w;
            for (int i = 1; i < normals.Count; i++)
            {
                if (normals[i].w < minDistance)
                {
                    minDistance = normals[i].w;
                    minFace = i;
                }
            }
        }

        if (iteration == maxIterations)
            Debug.LogWarning("EPA: Iteration limit reached, possible infinite loop");
    }

    private (List<Vector4> normals, int minFace) GetFaceNormals(List<SupportPoint> polytope, List<int> faces)
    {
        List<Vector4> normals = new List<Vector4>();
        int minTriangle = 0;
        float minDistance = float.MaxValue;

        for (int i = 0; i < faces.Count; i += 3)
        {
            Vector3 a = polytope[faces[i]].support;
            Vector3 b = polytope[faces[i + 1]].support;
            Vector3 c = polytope[faces[i + 2]].support;

            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            float distance = Vector3.Dot(normal, a);

            if (distance < 0)
            {
                normal *= -1;
                distance *= -1;
            }

            normals.Add(new Vector4(normal.x, normal.y, normal.z, distance));

            if (distance < minDistance)
            {
                minTriangle = i / 3;
                minDistance = distance;
            }
        }

        return (normals, minTriangle);
    }

    private void AddIfUniqueEdge(List<(int, int)> edges, List<int> faces, int a, int b)
    {
        if (edges.Contains((faces[b], faces[a])))
        {
            edges.Remove((faces[b], faces[a]));
        }
        else
        {
            edges.Add((faces[a], faces[b]));
        }
    }
}
