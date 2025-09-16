using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class PlacedObjectData
{
    public int id;
    public List<Vector3Int> occupiedPos;
    public Vector3 centerPos;
}

public class PlacementPersistence
{
    private const string FileName = "PlacedObjectsData.json";

    [Serializable]
    private class PlacedObjectListWrapper
    {
        public List<PlacedObjectData> objects = new();
    }

    private string FilePath => Path.Combine(Application.streamingAssetsPath, FileName);

    public void SavePlacedObject(int id, List<Vector3Int> position, Vector3 placedPos)
    {
        List<PlacedObjectData> placedObjects = LoadPlacedObjectsList();
        placedObjects.Add(new PlacedObjectData { id = id, occupiedPos = position, centerPos = placedPos });

        SaveToFile(placedObjects);

    }

    public List<PlacedObjectData> LoadPlacedObjectsList()
    {
        if (!File.Exists(FilePath)) return new List<PlacedObjectData>();

        string json = File.ReadAllText(FilePath);
        if (string.IsNullOrEmpty(json)) return new List<PlacedObjectData>();

        PlacedObjectListWrapper wrapper = JsonUtility.FromJson<PlacedObjectListWrapper>(json);
        return wrapper?.objects ?? new List<PlacedObjectData>();
    }

    public void LoadPlacedObjects(ObjectDataBaseSO database, Action<int, List<Vector3Int>, Vector3> instantiateCallback)
    {
        var placedObjects = LoadPlacedObjectsList();
        foreach (var obj in placedObjects)
        {
            int idx = database.objectData.FindIndex(x => x.id == obj.id);
            if (idx >= 0)
            {
                instantiateCallback(idx, obj.occupiedPos, obj.centerPos);
            }
        }
    }

    public void DeletePlacedObject(int id, List<Vector3Int> position)
    {
        List<PlacedObjectData> placedObjects = LoadPlacedObjectsList();
        PlacedObjectData objectToRemove = placedObjects.Find(o => o.id == id && ArePositionsEqual(o.occupiedPos, position));
        if (objectToRemove != null)
        {
            placedObjects.Remove(objectToRemove);
            SaveToFile(placedObjects);
        }
    }

    private bool ArePositionsEqual(List<Vector3Int> a, List<Vector3Int> b)
    {
        if (a == null || b == null || a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

    public void DeleteAllData()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }

    private void SaveToFile(List<PlacedObjectData> placedObjects)
    {
        var wrapper = new PlacedObjectListWrapper { objects = placedObjects };
        string json = JsonUtility.ToJson(wrapper, true); // true = pretty print
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }
        File.WriteAllText(FilePath, json);
        Debug.Log("Placed objects saved to " + FilePath);
    }
}
