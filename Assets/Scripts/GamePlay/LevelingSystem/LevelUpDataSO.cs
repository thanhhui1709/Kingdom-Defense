using UnityEngine;

[CreateAssetMenu(fileName = "LevelUpData", menuName = "Game/LevelUpData")]
public class LevelUpDataSO : ScriptableObject
{
    public LevelUpType type;
    public int level;
    public int cost;
    public int inGameBuyCost;
    public bool isUnlocked;
    public Sprite avatar;
    public GameObject prefab;
    public float spawnCoolDown;
}
