using System;
using System.Collections.Generic;
using UnityEngine;

public class GridData
{
    Dictionary<Vector3Int, GameObject> placedObjects = new();


    public void AddObject(Vector3Int gridPos, Vector2Int size, GameObject gameObject)
    {
        List<Vector3Int> positionToOccupy = CalculateOccupyCells(gridPos, size);

        foreach (var pos in positionToOccupy)
        {
            placedObjects[pos] = gameObject;
        }
    }
    public bool CanPlacePosition(Vector3Int gridPos, Vector2Int size, GameObject gameObject)
    {
        List<Vector3Int> positionToOccupy = CalculateOccupyCells(gridPos, size);

        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
            {
                GameObject existingObj = placedObjects[pos];


                if (!existingObj.CompareTag("Buildable")||gameObject.tag.Equals(existingObj.tag))
                {
                    Debug.LogError($"Cell {pos} is already occupied by a non-buildable object: {existingObj.name}");
                    return false;
                }
            }



        }
        return true;
    }

    internal GameObject GetObjectAtPosition(Vector3Int gridPos)
    {
        foreach (var pos in placedObjects.Keys)
        {
            if (pos == gridPos)
            {
                return placedObjects[pos];
            }
        }
        return null;
    }

    internal void RemoveObject(Vector3Int gridPos, Vector2Int size, GameObject objToDelete)
    {
        List<Vector3Int> positionToFree = CalculateOccupyCells(gridPos, size);
        foreach (var pos in positionToFree)
        {
            if (placedObjects.ContainsKey(pos) && placedObjects[pos] == objToDelete)
            {
                placedObjects.Remove(pos);
            }
        }
    }

    public List<Vector3Int> CalculateOccupyCells(Vector3Int gridPos, Vector2Int size)
    {
        List<Vector3Int> occupied = new List<Vector3Int>();

        int offsetX = size.x / 2;
        int offsetZ = size.y / 2;

        int startX = gridPos.x - offsetX;
        int startZ = gridPos.z - offsetZ;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                occupied.Add(new Vector3Int(startX + x, startZ + y));
            }
        }

        return occupied;
    }

}
