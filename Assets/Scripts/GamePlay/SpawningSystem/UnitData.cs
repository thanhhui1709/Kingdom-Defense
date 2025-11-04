using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Data", menuName = "Game Data/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public Sprite icon;
    public bool isUnlocked;
    public GameObject unitPrefab;
    public int cost;

}
