using UnityEngine;
using System.Collections.Generic;

public class LevelController : MonoBehaviour
{
    [Header("Level Config")]
    [SerializeField] private string towerNameKey;
    [SerializeField] private int evolutionLevel = 1;
    [SerializeField] private List<LevelStats> statsList = new List<LevelStats>();

    private Stats stats;
    private int subLevel = 0;
    private bool isReadyToEvolve = false;

    void Awake()
    {
        stats = GetComponent<Stats>();
    }

    void Start()
    {
        UpdateLevelState();
    }

    public bool IsReadyToEvolve()
    {
        return isReadyToEvolve;
    }

    public bool IsAtMaxEvolution()
    {
        return LevelUpManager.Instance.GetNextLevelPrefab(LevelUpType.Tower, towerNameKey, evolutionLevel) == null;
    }

    public void LevelUp()
    {
        if (isReadyToEvolve)
        {
            EvolveTower();
        }
        else if (subLevel < statsList.Count)
        {
            ApplySubLevelStats();
        }
        else
        {
            Debug.Log("Đã đạt cấp tối đa cho phiên bản này.");
        }
    }

    /// <summary>
    /// Cộng chỉ số (lên cấp nội bộ)
    /// </summary>
    private void ApplySubLevelStats()
    {
        LevelStats levelStats = statsList[subLevel];

        // --- KIỂM TRA TIỀN (SUB-LEVEL) ---
        if (!InGameMoney.Instance.CheckBalance(levelStats.cost))
        {
            Debug.Log("Không đủ tiền để nâng cấp (sub-level)!");
            return; 
        }

        // Đủ tiền, trừ tiền
        InGameMoney.Instance.SubMoney(levelStats.cost);

        // Cập nhật tổng tiền đầu tư
        stats.TotalInvestedMoney += levelStats.cost;
        // --- KẾT THÚC LOGIC TIỀN ---

        // Cộng chỉ số
        stats.Ammor += levelStats.ammor;
        stats.Heath += levelStats.heath;
        stats.AttackDamage += levelStats.attackDamage;
        stats.Money += levelStats.cost; // Cộng chi phí xây dựng cơ bản
        stats.AttackSpeed += levelStats.attackSpeed;
        stats.MoveSpeed += levelStats.moveSpeed;
        stats.AttackRange += levelStats.attackRange;
        stats.TriggerRange += levelStats.triggerRange;


        subLevel++;
        UpdateLevelState();

        // Bắn sự kiện để UI cập nhật (vì prefab không đổi)
        GameEvent.Instance.OnTriggerTowerLevelUp(gameObject);
    }

    /// <summary>
    /// Tiến hóa (thay prefab)
    /// </summary>
    private void EvolveTower()
    {
        // Lấy chi phí tiến hóa TRƯỚC
        int cost = LevelUpManager.Instance.GetLevelUpCost(towerNameKey, evolutionLevel);

        // --- KIỂM TRA TIỀN (EVOLVE) ---
        if (cost == 0) // Không tìm thấy cấp tiếp theo
        {
            Debug.Log("Đã max cấp, không thể tiến hóa.");
            return;
        }
        if (!InGameMoney.Instance.CheckBalance(cost))
        {
            Debug.Log("Không đủ tiền để tiến hóa (evolve)!");
            return; // Dừng lại
        }
        // --- KẾT THÚC LOGIC TIỀN ---

        GameObject nextLevelPrefab = LevelUpManager.Instance.GetNextLevelPrefab(
            LevelUpType.Tower, towerNameKey, evolutionLevel
        );

        if (nextLevelPrefab != null)
        {
            // Đã kiểm tra, giờ trừ tiền
            InGameMoney.Instance.SubMoney(cost);

            // Spawn trụ mới
            GameObject newTower = ObjectPoolManager.SpawnObject(
                nextLevelPrefab, transform.position, transform.rotation, ObjectPoolManager.PoolType.Tower
            );
            newTower.tag = "Tower";

            // --- CHUYỂN TIỀN ĐẦU TƯ SANG TRỤ MỚI ---
            Stats newTowerStats = newTower.GetComponent<Stats>();
            if (newTowerStats != null)
            {
                // Tiền của trụ mới = tiền của trụ cũ + tiền vừa tiêu
                newTowerStats.TotalInvestedMoney = stats.TotalInvestedMoney + cost;
            }
            // --- KẾT THÚC ---

            GameEvent.Instance.OnTriggerTowerLevelUp(newTower);
            ObjectPoolManager.ReturnObject(gameObject);
        }
    }

    /// <summary>
    /// Lấy chi phí nâng cấp cho lần tiếp theo
    /// </summary>
    public int GetNextLevelCost()
    {
        if (isReadyToEvolve)
        {
            return LevelUpManager.Instance.GetLevelUpCost(towerNameKey, evolutionLevel);
        }
        else if (subLevel < statsList.Count)
        {
            return statsList[subLevel].cost;
        }
        return 0; // Max
    }

    private void UpdateLevelState()
    {
        isReadyToEvolve = (subLevel >= statsList.Count);
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