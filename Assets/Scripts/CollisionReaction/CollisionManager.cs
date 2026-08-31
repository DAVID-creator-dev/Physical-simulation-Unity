using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    private DynamicTree tree;
    private GJK gjk;

    private Dictionary<Rigidbody, int> objectIds = new Dictionary<Rigidbody, int>();
    private List<CollisionPair> pairs = new List<CollisionPair>();

    public void RegisterObject(Rigidbody obj)
    {
        int id = tree.Insert(obj);
        objectIds[obj] = id;
    }

    public void UnregisterObject(Rigidbody obj)
    {
        if (!objectIds.TryGetValue(obj, out int id)) return;
        tree.Remove(id);
        objectIds.Remove(obj);
    }

    public void Start()
    {
        tree = GetComponent<DynamicTree>();
        if (tree == null)
            tree = gameObject.AddComponent<DynamicTree>();

        gjk = GetComponent<GJK>();
        if (gjk == null)
            gjk = gameObject.AddComponent<GJK>();

        foreach (var obj in Rigidbody.All)
        {
            RegisterObject(obj);
        }
    }

    public void Update()
    {
        pairs.Clear();

        foreach (var kvp in objectIds)
        {
            var physicObj = kvp.Key;
            kvp.Key.UpdateAABB();
            tree.UpdateTree(kvp.Value);
        }

        tree.DebugDrawTree();

        tree.QueryPairs(tree.root, tree.root, pairs);

        foreach (var pair in pairs)
        {
            if (gjk.NarrowPhase(pair.A.collider, pair.B.collider) && !pair.hasCollision)
            {
                pair.CollisionReply(gjk);
            }
        }
    }
}