using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
  
    public Vector3 position => transform.position;


    public List<PathNode> neighbors;

    void Awake()
    {
        neighbors = new List<PathNode>();
      
        FindNeighbor();
    }

    private void FindNeighbor()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5.5f);
        foreach (var hitCollider in hitColliders)
        {
            PathNode neighborNode = hitCollider.GetComponent<PathNode>();
            if (neighborNode != null && neighborNode != this)
            {
                neighbors.Add(neighborNode);
            }
        }
    }
}
