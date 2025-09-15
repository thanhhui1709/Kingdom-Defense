using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class PlacedObjectData
{
    public int id;
    public List<Vector3Int> occupiedPos;
    public Vector3 centerPos;
}

public class PlacementPersistence
{
    private const string PlacedObjectsKey = "PlacedObjectsData";

    [Serializable]
    private class PlacedObjectListWrapper
    {
        public List<PlacedObjectData> objects = new();
    }

    public void SavePlacedObject(int id, List<Vector3Int> position, Vector3 placedPos)
    {
        List<PlacedObjectData> placedObjects = LoadPlacedObjectsList();
        placedObjects.Add(new PlacedObjectData { id = id, occupiedPos = position, centerPos = placedPos });
        string json = JsonUtility.ToJson(new PlacedObjectListWrapper { objects = placedObjects });
#if UNITY_EDITOR
        EditorPrefs.SetString(PlacedObjectsKey, json);
#endif
    }

    public List<PlacedObjectData> LoadPlacedObjectsList()
    {
#if UNITY_EDITOR
        string json = EditorPrefs.GetString(PlacedObjectsKey, "");
        if (string.IsNullOrEmpty(json)) return new List<PlacedObjectData>();
        PlacedObjectListWrapper wrapper = JsonUtility.FromJson<PlacedObjectListWrapper>(json);
        return wrapper?.objects ?? new List<PlacedObjectData>();
#else
        return new List<PlacedObjectData>();
#endif
    }

    public void LoadPlacedObjects(ObjectDataBaseSO database, Action<int, List<Vector3Int>, Vector3> instantiateCallback)
    {
#if UNITY_EDITOR
        var placedObjects = LoadPlacedObjectsList();
        foreach (var obj in placedObjects)
        {
            int idx = database.objectData.FindIndex(x => x.id == obj.id);
            if (idx >= 0)
            {
                instantiateCallback(idx, obj.occupiedPos, obj.centerPos);
            }
        }
#endif
    }
    public void DeletePlacedObject(int id, List<Vector3Int> position)
    {

        List<PlacedObjectData> placedObjects = LoadPlacedObjectsList();
        PlacedObjectData objectToRemove = placedObjects.Find(o => o.id == id && ArePositionsEqual(o.occupiedPos, position));
        if (objectToRemove != null)
        {
            placedObjects.Remove(objectToRemove);
        }
        string json = JsonUtility.ToJson(new PlacedObjectListWrapper { objects = placedObjects });
#if UNITY_EDITOR
        EditorPrefs.SetString(PlacedObjectsKey, json);
#endif
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
#if UNITY_EDITOR
        EditorPrefs.DeleteKey(PlacedObjectsKey);
#endif
    }
}
