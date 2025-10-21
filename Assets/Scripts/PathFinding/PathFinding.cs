using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding 
{
    public List<PathNode> FindPath(PathNode start, PathNode goal)
    {
        var openSet = new PriorityQueue<PathNode>();
        var cameFrom = new Dictionary<PathNode, PathNode>();

        var gScore = new Dictionary<PathNode, float>();
        var fScore = new Dictionary<PathNode, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            PathNode current = openSet.Dequeue();

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach (PathNode neighbor in current.neighbors)
            {
                float tentative_gScore = gScore[current] + Vector3.Distance(current.position, neighbor.position);

                if (!gScore.ContainsKey(neighbor) || tentative_gScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative_gScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }
        }
        return null; // không tìm th?y
    }

    private float Heuristic(PathNode a, PathNode b)
    {
        return Vector3.Distance(a.position, b.position); // Euclidean
    }
    private List<PathNode> ReconstructPath(Dictionary<PathNode, PathNode> cameFrom, PathNode current)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(current);
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}
