using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class LevelUpManager : MonoBehaviour
{
    [SerializeField]
    private List<LevelUpDataSO> ArcherTowerData = new List<LevelUpDataSO>();
    [SerializeField]
    private List<LevelUpDataSO> BallistaTowerData = new List<LevelUpDataSO>();
    [SerializeField]
    private List<LevelUpDataSO> CannonTowerData = new List<LevelUpDataSO>();
    [SerializeField]
    private List<LevelUpDataSO> WizardTowerData = new List<LevelUpDataSO>();
    [SerializeField]
    private List<LevelUpDataSO> PoisonTowerData = new List<LevelUpDataSO>();
    [SerializeField]
    private List<LevelUpDataSO> BarbarianData = new List<LevelUpDataSO>();
    [SerializeField]
    private List<LevelUpDataSO> ArcherData = new List<LevelUpDataSO>();
    [SerializeField]
    private List<LevelUpDataSO> KnightData = new List<LevelUpDataSO>();
    [SerializeField]
    private List<LevelUpDataSO> MageData = new List<LevelUpDataSO>();

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
        List<LevelUpDataSO> list = GetListFromName(name);
        if (list == null)
        {
            Debug.LogWarning("Không tìm thấy LevelUpData list cho: " + name);
            return null;
        }


        LevelUpDataSO nextLevelData = list.Find(data => data.type == type && data.level == currentLevel + 1);

        if (nextLevelData == null)
        {
            Debug.Log("Không tìm thấy cấp tiếp theo.");
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
        List<LevelUpDataSO> list = GetListFromName(name);
        if (list == null) return 0;

        LevelUpDataSO nextLevelData = list.Find(data => data.level == currentLevel + 1 &&data.isUnlocked == true);

        // Trả về cost nếu tìm thấy, ngược lại trả về 0
        return nextLevelData?.inGameBuyCost ?? 0;
    }

    private List<LevelUpDataSO> GetListFromName(string name)
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
    public List<List<LevelUpDataSO>> GetLevelUpDatas()
    {
        return new List<List<LevelUpDataSO>>()
        {
            ArcherTowerData,
            BallistaTowerData,
            CannonTowerData,
            WizardTowerData,
            PoisonTowerData,
            BarbarianData,
            ArcherData,
            KnightData,
            MageData
        };
    }
}



public enum LevelUpType
{
    Tower,
    Unit,
    Obstacle
}