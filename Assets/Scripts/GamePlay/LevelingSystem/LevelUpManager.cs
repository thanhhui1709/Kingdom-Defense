using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class LevelUpManager : MonoBehaviour
{
    [SerializeField]
    private List<LevelUpData> ArcherTowerData = new List<LevelUpData>();
    [SerializeField]
    private List<LevelUpData> BallistaTowerData = new List<LevelUpData>();
    [SerializeField]
    private List<LevelUpData> CannonTowerData = new List<LevelUpData>();
    [SerializeField]
    private List<LevelUpData> WizardTowerData = new List<LevelUpData>();
    [SerializeField]
    private List<LevelUpData> PoisonTowerData = new List<LevelUpData>();
    [SerializeField]
    private List<LevelUpData> BarbarianData = new List<LevelUpData>();
    [SerializeField]
    private List<LevelUpData> ArcherData = new List<LevelUpData>();
    [SerializeField]
    private List<LevelUpData> KnightData = new List<LevelUpData>();
    [SerializeField]
    private List<LevelUpData> MageData = new List<LevelUpData>();

    public static LevelUpManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Lấy Prefab của cấp tiếp theo
    /// </summary>
    public GameObject GetNextLevelPrefab(LevelUpType type, string name, int currentLevel)
    {
        List<LevelUpData> list = GetListFromName(name);
        if (list == null)
        {
            Debug.LogWarning("Không tìm thấy LevelUpData list cho: " + name);
            return null;
        }

        // Tối ưu: Dùng Linq.Find cho nhanh
        LevelUpData nextLevelData = list.Find(data => data.type == type && data.level == currentLevel + 1);

        if (nextLevelData == null)
        {
            // Debug.Log("Không tìm thấy cấp tiếp theo.");
            return null;
        }

        if (nextLevelData.isUnlocked)
        {
            return nextLevelData.prefab;
        }

        Debug.Log("Cấp tiếp theo chưa được mở khóa.");
        return null;
    }

    /// <summary>
    /// Lấy chi phí của cấp tiếp theo
    /// </summary>
    public int GetLevelUpCost(string name, int currentLevel)
    {
        List<LevelUpData> list = GetListFromName(name);
        if (list == null) return 0;

        LevelUpData nextLevelData = list.Find(data => data.level == currentLevel + 1);

        // Trả về cost nếu tìm thấy, ngược lại trả về 0
        return nextLevelData?.cost ?? 0;
    }

    private List<LevelUpData> GetListFromName(string name)
    {
        switch (name)
        {
            case Const.ARCHER_TOWER_NAME:
                return ArcherTowerData;
            case Const.BALLISTA_TOWER_NAME:
                return BallistaTowerData;
            case Const.CANNON_TOWER_NAME:
                return CannonTowerData;
            case Const.POISON_TOWER_NAME:
                return PoisonTowerData;
            case Const.WIZARD_TOWER_NAME:
                return WizardTowerData;
            case Const.UNIT_BARBARIAN_NAME:
                return BarbarianData;
            case Const.UNIT_ARCHER_NAME:
                return ArcherData;
            case Const.UNIT_KNIGHT_NAME:
                return KnightData;
            case Const.UNIT_MAGE_NAME:
                return MageData;
            default:
                return null;
        }
    }
}

[System.Serializable]
public class LevelUpData
{
    public LevelUpType type;
    public int level;
    public int cost;
    public bool isUnlocked;
    public GameObject prefab;
}

public enum LevelUpType
{
    Tower,
    Unit,
    Obstacle
}