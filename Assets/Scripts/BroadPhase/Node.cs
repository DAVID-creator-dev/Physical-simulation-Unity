using UnityEngine;

public class Node
{
    public int id;
    public AABB box;
    public Rigidbody obj; 

    public Node parent;
    public Node left;
    public Node right;

    public bool isLeaf => left == null && right == null;

    public Node(int _id, AABB _box, Rigidbody _obj)
    {
        id = _id;
        box = _box;
        obj = _obj;
    }
}
