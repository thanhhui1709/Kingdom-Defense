using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    public List<GameObject> neighborObjects = new();
    public Vector3 position => transform.position;


    [HideInInspector]
    public List<PathNode> neighbors;

    void Awake()
    {
        neighbors = new List<PathNode>();
        foreach (var obj in neighborObjects)
        {
            if (obj != null)
                neighbors.Add(obj.GetComponent<PathNode>());
        }
    }
}
