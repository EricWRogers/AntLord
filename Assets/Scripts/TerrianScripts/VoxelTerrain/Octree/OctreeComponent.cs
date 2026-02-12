using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OctreeComponent : MonoBehaviour
{
    public float size;
    public int depth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        var octree = new Octree<int>(this.transform.position, size, depth);
        
        DrawNode(octree.GetRoot());
    }

    private void DrawNode(Octree<int>.OctreeNode<int> node)
    {
        if (node.IsLeaf())
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.blue;
            foreach (var subnode in node.Nodes)
            {
                
            }
        }
    }
}
