using System.Collections.Generic;
using UnityEngine;

public class Rigidbody : MonoBehaviour
{
    public static readonly List<Rigidbody> All = new List<Rigidbody>();

    // --- Collider ---
    public AABB box;
    [HideInInspector] public ConvexCollider collider; 
    
    private MeshRenderer mesh;

    // --- Physical properties ---
    public float m = 1f;                  
    public float restitution = 0.5f;      
    public float staticFriction;          
    public float dynamicFriction;         
    public bool isStatic = false;

    // --- Dynamic properties ---
    public Vector3 linearVelocity;       
    public Vector3 angularVelocity;

    // --- Rotation properties ---
    public float angularDamping = 0.98f;  
    public float linearDamping = 0.98f; 
    public Matrix4x4 inertiaTensor;

    // --- Physical constants ---
    private float g = -9.81f;

    private void OnDestroy() => All.Remove(this);
    public float GetMass() => isStatic ? 0f : m;
    public float GetInvMass() => isStatic ? 0f : (m == 0f ? float.PositiveInfinity : 1f / m);

    private void Awake()
    {
        All.Add(this);

        collider = GetComponent<ConvexCollider>();
        if (collider == null)
        {
            Debug.LogError("ConvexCollider manquant sur " + gameObject.name);
            enabled = false;
            return;
        }

        mesh = collider.GetComponent<MeshRenderer>();
        if (mesh == null)
        {
            Debug.LogWarning("Mesh non trouvé pour le collider sur " + gameObject.name);
        }

        box = new AABB(Vector3.zero, Vector3.zero);
        UpdateAABB();

        UpdateInertiaTensor();
    }

    public void UpdateAABB()
    {
        Bounds bounds = mesh.bounds;
        box.min = bounds.min;
        box.max = bounds.max;
    }

    void UpdateAngularVelocity()
    {
        if (angularVelocity.sqrMagnitude < 1e-6f)
        {
            angularVelocity = Vector3.zero;
            return;
        }
        transform.rotation = Quaternion.Euler(angularVelocity * Mathf.Rad2Deg * Time.deltaTime) * transform.rotation;
    }

    void UpdateLinearVelocity() 
    {
        linearVelocity += new Vector3(0, g, 0) * Time.deltaTime;
        transform.position += linearVelocity * Time.deltaTime;
    }

    public void UpdateInertiaTensor()
    {
        if (mesh != null && collider != null)
            inertiaTensor = InertiaMoment.Calculate(collider.shape, m, mesh.bounds);
    }

    void Update()
    {
        if (isStatic)
            return;

        UpdateLinearVelocity(); 
        UpdateAngularVelocity();
        UpdateAABB();
    }
}
