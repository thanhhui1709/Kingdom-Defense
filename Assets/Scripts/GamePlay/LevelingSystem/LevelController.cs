using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    private Stats stats;

    [SerializeField]
    [Tooltip("Cấp độ hiện tại của tháp phòng thủ.Nếu lên cấp thì sẽ tăng chỉ số,nếu max cấp sẽ tiến hóa")]
    private int towerLevel = 1;

    [SerializeField]
    private List<LevelStats> statsList = new List<LevelStats>();
    private int currentLevel = 0;


    void Start()
    {
        stats = GetComponent<Stats>();
    }


    public void LevelUp()
    {
        
        if (currentLevel == statsList.Count)
        {
            Upgrade();
            return;
        }
        LevelStats levelStats = statsList[currentLevel];
        stats.Ammor += levelStats.ammor;
        stats.Heath += levelStats.heath;
        stats.AttackDamage += levelStats.attackDamage;
        stats.AttackSpeed += levelStats.attackSpeed;
        stats.MoveSpeed += levelStats.moveSpeed;
        stats.AttackRange += levelStats.attackRange;
        stats.TriggerRange += levelStats.triggerRange;
        currentLevel++;


    }

    private void Upgrade()
    {
        GameObject nextLevelObject = LevelUpManager.Instance.GetLevelUpGameObject(LevelUpManager.LevelUpType.Tower, GetTowerName(gameObject.name.Substring(0, gameObject.name.Length - 11)), towerLevel);

        if (nextLevelObject != null)
        {
            GameObject newTower = ObjectPoolManager.SpawnObject(nextLevelObject, transform.position, transform.rotation, ObjectPoolManager.PoolType.Tower);
            ObjectPoolManager.ReturnObject(gameObject);
        }
    }
    private string GetTowerName(string name)
    {
        switch (name)
        {
            case "Archer_Tower":
                return Const.ARCHER_TOWER_NAME;
            case "Ballista_Tower":
                return Const.BALLISTA_TOWER_NAME;
            case "Cannon_Tower":
                return Const.CANNON_TOWER_NAME;
            case "Poison_Tower":
                return Const.POISON_TOWER_NAME;
            case "Wizard_Tower":
                return Const.WIZARD_TOWER_NAME;
            default:
                return "";
        }
    }
}
[System.Serializable]
public class LevelStats
{
    public int cost;
    public float heath;
    public float ammor;
    public float attackDamage;
    public float attackSpeed;
    public float moveSpeed;
    public float attackRange;
    public float triggerRange;

}
