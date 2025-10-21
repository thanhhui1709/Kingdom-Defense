using UnityEngine;
using System.Collections;
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

    public static LevelUpManager Instance=new LevelUpManager();
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
    }

    // Retrieves LevelUpData based on type and current level ==> return game object of next level
    public GameObject GetLevelUpGameObject(LevelUpType type, string name, int currentLevel)
    {
        List<LevelUpData> levelUpDataList = GetListFromName(name);
        if (levelUpDataList == null)
        {

            Debug.Log("No data list found for name: " + name);
            return null;
        }
        // 
        if (currentLevel >= levelUpDataList.Max(x => x.level))
        {
            Debug.Log("Already at max level.");
            return null;
        }
        foreach (LevelUpData data in levelUpDataList)
        {
            if (data.type == type && data.level == currentLevel + 1)
            {
                if (data.isUnlocked)
                {
                    return data.prefab;

                }
                Debug.Log("Level up not unlocked yet.");
                return null;
            }
        }
        return null;
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
            default:
                return null;
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
}
