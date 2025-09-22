using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class Tile : MonoBehaviour
{
    [SerializeField] private Grid grid;
    public Dictionary<TileType, List<GameObject>> tiles = new();
    public HashSet<Vector3Int> freeCell = new HashSet<Vector3Int>();
    private void Start()
    {
        tiles.Add(TileType.Vertical, new List<GameObject>());
        tiles.Add(TileType.Horizontal, new List<GameObject>());
        tiles.Add(TileType.Intersection, new List<GameObject>());
    }
    public void LoadTile()
    {
        GetDefaultTile();
        GetAllFreeCells();
    }
    private void GetDefaultTile()
    {
        List<GameObject> existedTile = GameObject.FindGameObjectsWithTag("Road").ToList();
        foreach (var tile in existedTile)
        {
            tile.transform.parent = null;
            tile.transform.parent = this.transform;
            tiles.FirstOrDefault().Value.Add(tile);
        }
    }
    private void GetAllFreeCells()
    {
        if (tiles.Count == 0)
        {
            return;
        }
        foreach (var tile in tiles.Keys)
        {
            if (tiles[tile].Count == 0) continue;
            foreach (var obj in tiles[tile])
            {
                
                Vector3Int cellPos = grid.WorldToCell(obj.transform.position);
                cellPos.y = 0;

                if (tile == TileType.Horizontal)
                {
                    freeCell.Add(cellPos + new Vector3Int(0, 0, 1));
                    freeCell.Add(cellPos + new Vector3Int(0, 0, -1));
                }
                else if (tile == TileType.Vertical)
                {
                    freeCell.Add(cellPos + new Vector3Int(1, 0, 0));
                    freeCell.Add(cellPos + new Vector3Int(-1, 0, 0));
                }
                else if (tile == TileType.Intersection)
                {
                    freeCell.Add(cellPos + new Vector3Int(1, 0, 0));
                    freeCell.Add(cellPos + new Vector3Int(-1, 0, 0));
                    freeCell.Add(cellPos + new Vector3Int(0, 0, 1));
                    freeCell.Add(cellPos + new Vector3Int(0, 0, -1));
                }
            }
        }
    }
    public bool IsCellFree(Vector3Int cellPos)
    {
        return freeCell.Contains(cellPos);
    }
}
public enum TileType
{
    Horizontal,
    Vertical,
    Intersection,
}
