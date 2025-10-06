using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class Tile : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private int tileLength;
    [SerializeField] private int maxPathLength = 99;
    public Dictionary<Vector3Int, GameObject> tiles = new();
    public HashSet<Vector3Int> freeCell = new();
    public HashSet<Vector3Int> validEndPoints = new();

    private void Start()
    {
    }

    public void LoadTile()
    {
        GetTileInScene();
        GetAllFreeCellsAndOccupiedCells();
        tileLength = tiles.Count;
        if (tiles.Count > 0)
            GetAllValidEndPoints(tiles.Keys.Last(), new Vector2Int(2, 1));
    }

    private void GetTileInScene()
    {
        List<GameObject> existedTile = GameObject.FindGameObjectsWithTag("Road").ToList();
        foreach (var tile in existedTile)
        {
            tile.transform.parent = null;
            tile.transform.parent = this.transform;
            Vector3Int occupiedPos = grid.WorldToCell(tile.transform.position);
            occupiedPos.y = 0;
            if (!tiles.ContainsKey(occupiedPos))
                tiles.Add(occupiedPos, tile);
        }
    }

    private void GetAllValidEndPoints(Vector3Int centerPos, Vector2Int size)
    {
        if (size.x <= 0 || size.y <= 0)
        {
            Debug.LogError("Invalid size for valid end points");
            return;
        }
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                validEndPoints.Add(centerPos + new Vector3Int(i, 0, j));
                validEndPoints.Add(centerPos + new Vector3Int(-i, 0, j));
            }
        }
    }

    private void GetAllFreeCellsAndOccupiedCells()
    {
        if (tiles.Count == 0) return;

        foreach (var tile in tiles.Keys)
        {
            Vector3Int cellPos = tile;
            cellPos.y = 0;
            freeCell.Add(cellPos + new Vector3Int(1, 0, 0));
            freeCell.Add(cellPos + new Vector3Int(-1, 0, 0));
            freeCell.Add(cellPos + new Vector3Int(0, 0, 1));
            freeCell.Add(cellPos + new Vector3Int(0, 0, -1));
        }
    }

    public bool IsCellFree(Vector3Int cellPos)
    {
        return freeCell.Contains(cellPos);
    }

    public bool CheckValidDrawRoad(Dictionary<Vector3Int, GameObject> selectedObjects)
    {
        if (selectedObjects.Count < 2) return false;

        Vector3Int start = selectedObjects.Keys.First();
        Vector3Int end = selectedObjects.Keys.Last();

        // Start phải nằm trong path đã tồn tại
        if (!tiles.ContainsKey(start))
        {
            Debug.LogError("Path must start on existing road");
            return false;
        }

        // Check liên thông duy nhất từ start -> end trong vùng selected
        if (!IsConnectedUniquePath(selectedObjects.Keys.ToList(), start, end))
        {
            Debug.LogError("Invalid Path: not a single unique connected path");
            return false;
        }

        return true;
    }

    private bool IsConnectedUniquePath(List<Vector3Int> cells, Vector3Int start, Vector3Int end)
    {
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> q = new Queue<Vector3Int>();
        q.Enqueue(start);
        visited.Add(start);

        Vector3Int[] dirs = { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur == end) return true;

            foreach (var d in dirs)
            {
                var next = cur + d;
                if (cells.Contains(next) && !visited.Contains(next))
                {
                    visited.Add(next);
                    q.Enqueue(next);
                }
            }
        }
        return false;
    }

    public bool HasUniquePath()
    {
        if (tiles.Count == 0 || validEndPoints.Count == 0) return false;

        Vector3Int start = tiles.Keys.First();
        Vector3Int end = validEndPoints.First();

        int pathCount = CountPathsBFS(start, end);
        return pathCount == 1;
    }

    private int CountPathsBFS(Vector3Int start, Vector3Int end)
    {
        Queue<Vector3Int> q = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        q.Enqueue(start);
        visited.Add(start);

        Vector3Int[] dirs = { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur == end) return 1; // tìm thấy 1 đường

            foreach (var d in dirs)
            {
                var next = cur + d;
                if (tiles.ContainsKey(next) && !visited.Contains(next))
                {
                    visited.Add(next);
                    q.Enqueue(next);
                }
            }
        }
        return 0;
    }

    public Dictionary<Vector3Int, GameObject> MergePath(Dictionary<Vector3Int, GameObject> selectedObjects)
    {
        Dictionary<Vector3Int, GameObject> newTiles = new Dictionary<Vector3Int, GameObject>(tiles);

        foreach (var obj in selectedObjects)
        {
            if (!newTiles.ContainsKey(obj.Key))
            {
                newTiles.Add(obj.Key, obj.Value);
            }
            else
            {
                // replace nếu cần
                newTiles[obj.Key] = obj.Value;
            }
        }

        tiles = newTiles;
        return tiles;
    }
}
