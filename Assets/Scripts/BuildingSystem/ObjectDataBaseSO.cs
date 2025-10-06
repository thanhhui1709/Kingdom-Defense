using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using System;
[CreateAssetMenu(fileName = "ObjectDataBase", menuName = "ScriptableObjects/ObjectDataBase")]

public class ObjectDataBaseSO : ScriptableObject
{
    public List<ObjectData> objectData;

}
[System.Serializable]
public class ObjectData
{
    public string name;
    public int id;
    public GameObject prefab;
    public Vector2Int size = Vector2Int.one;
}
