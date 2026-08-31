using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class DynamicTree : MonoBehaviour
{
    public Node root { get; private set; }
    private int nextId = 0;
    private Dictionary<int, Node> nodesById = new Dictionary<int, Node>();
    public float margin = 0.1f;

    public int Insert(Rigidbody obj)
    {
        obj.UpdateAABB();
        AABB newAABB = obj.box.Expanded(margin);

        int id = nextId++;
        Node node = new Node(id, newAABB, obj);

        nodesById[id] = node;  
        InsertNode(node);

        return id;
    }

    public void Remove(int id)
    {
        if (!nodesById.TryGetValue(id, out Node node))
            return;

        if (node == root)
        {
            nodesById.Remove(node.id);
            root = null;
            return;
        }

        Node parent = node.parent;
        Node grandParent = parent.parent;
        Node sibling = (parent.left == node) ? parent.right : parent.left;

        if (grandParent == null)
        {
            root = sibling;
            sibling.parent = null;
        }
        else
        {
            if (grandParent.left == parent) grandParent.left = sibling;
            else grandParent.right = sibling;
            sibling.parent = grandParent;
        }

        nodesById.Remove(node.id);
        nodesById.Remove(parent.id);

        Refit(grandParent);
    }

    public void UpdateTree(int id)
    {
        if (!nodesById.TryGetValue(id, out Node node))
            return;

        if (node.obj == null)
            return;

        node.obj.UpdateAABB();
        AABB newBox = node.obj.box.Expanded(margin);

        if (node.box.Contains(newBox))
        {
            node.box = newBox;
            Refit(node.parent);
        }
        else
        {
            Rigidbody obj = node.obj;
            Remove(id);
            Node newNode = new Node(id, newBox, obj);
            nodesById[id] = newNode;
            InsertNode(newNode);
        }
    }

    private void InsertNode(Node node)
    {
        if (root == null)
        {
            root = node;
            node.parent = null;
            return;
        }

        Node current = root;
        while (!current.isLeaf)
        {
            float costLeft = EnlargedVolume(current.left.box, node.box);
            float costRight = EnlargedVolume(current.right.box, node.box);

            current = (costLeft < costRight) ? current.left : current.right;
        }

        int newParentId = nextId++;
        Node newParent = new Node(newParentId, AABB.Union(current.box, node.box), null);

        newParent.left = current;
        newParent.right = node;
        newParent.parent = current.parent;

        current.parent = newParent;
        node.parent = newParent;

        if (newParent.parent == null)
        {
            root = newParent;
        }
        else if (newParent.parent.left == current)
        {
            newParent.parent.left = newParent;
        }
        else
        {
            newParent.parent.right = newParent;
        }

        nodesById[newParentId] = newParent;

        Refit(newParent);
    }

    public void QueryPairs(Node a, Node b, List<CollisionPair> pairs)
    {
        if (a == null || b == null)
            return;

        if (!a.box.CheckOverlap(b.box))
        {
            return;
        }

        if (a.isLeaf && b.isLeaf && a != b)
        {
            if (a.isLeaf && b.isLeaf)
            {
                if (a == b) return;

                int idA = a.obj.GetInstanceID();
                int idB = b.obj.GetInstanceID();

                if (idA < idB)
                {
                    pairs.Add(new CollisionPair(a.obj, b.obj));
                }

                return;
            }
        }
        else if (!a.isLeaf && !b.isLeaf)
        {
            QueryPairs(a.left, b.left, pairs);
            QueryPairs(a.left, b.right, pairs);
            QueryPairs(a.right, b.left, pairs);
            QueryPairs(a.right, b.right, pairs);
        }
        else if (!a.isLeaf)
        {
            QueryPairs(a.left, b, pairs);
            QueryPairs(a.right, b, pairs);
        }
        else if (!b.isLeaf)
        {
            QueryPairs(a, b.left, pairs);
            QueryPairs(a, b.right, pairs);
        }
    }
    public Node GetNode(int id)
    {
        nodesById.TryGetValue(id, out Node node);
        return node;
    }

    private void Refit(Node node)
    {
        while (node != null)
        {
            if (node.isLeaf)
            {
                if (node.obj != null)
                    node.box = node.obj.box.Expanded(margin);
            }
            else
            {
                node.box = AABB.Union(node.left.box, node.right.box);
            }
            node = node.parent;
        }
    }

    private float EnlargedVolume(AABB a, AABB b)
    {
        AABB u = AABB.Union(a, b);
        return u.Volume() - a.Volume();
    }

    public void DebugDrawTree()
    {
        if (root == null) return;
        DrawNodeRecursive(root);
    }

    private void DrawNodeRecursive(Node node)
    {
        if (node == null) return;

        Color color = node.isLeaf ? Color.green : Color.yellow;
        DebugDraw3D.DrawAABB(node.box, color, 0f);

        DrawNodeRecursive(node.left);
        DrawNodeRecursive(node.right);
    }
}