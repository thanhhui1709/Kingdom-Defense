using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathNodeManager : MonoBehaviour
{
    public static PathNodeManager Instance { get; private set; }
    private List<PathNode> allNodes = new List<PathNode>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // Tải tất cả các node trong Scene vào danh sách
        allNodes = FindObjectsOfType<PathNode>().ToList();
        if (allNodes.Count == 0)
        {
            Debug.LogError("PathNodeManager không tìm thấy PathNode nào!");
        }
    }

    /// <summary>
    /// Tìm PathNode gần nhất với một vị trí (Vector3)
    /// </summary>
    public PathNode FindClosestNode(Vector3 position)
    {
        PathNode closest = null;
        float minDistance = float.MaxValue;

        foreach (PathNode node in allNodes)
        {
            float dist = Vector3.Distance(position, node.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = node;
            }
        }
        return closest;
    }
}

