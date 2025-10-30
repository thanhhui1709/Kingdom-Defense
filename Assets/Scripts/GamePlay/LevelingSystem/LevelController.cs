using UnityEngine;
using System.Collections.Generic;

public class LevelController : MonoBehaviour
{
    [Header("Level Config")]
    [Tooltip("Key định danh cho trụ này, ví dụ: Const.ARCHER_TOWER_NAME")]
    [SerializeField] private string towerNameKey; // DÙNG CÁI NÀY, ĐỪNG DÙNG SUBSTRING

    [Tooltip("Cấp 'tiến hóa' hiện tại, ví dụ: 1, 2, 3...")]
    [SerializeField] private int evolutionLevel = 1;

    [Tooltip("Danh sách các chỉ số cộng thêm cho mỗi 'cấp' (sub-level)")]
    [SerializeField] private List<LevelStats> statsList = new List<LevelStats>();

    private Stats stats;
    private int subLevel = 0; // Cấp nội bộ (0, 1, 2...)
    private bool isReadyToEvolve = false; // Đã max cấp, sẵn sàng tiến hóa

    void Awake() // Dùng Awake để đảm bảo Stats được lấy trước
    {
        stats = GetComponent<Stats>();
    }

    void Start()
    {
        // Khởi tạo trạng thái
        UpdateLevelState();
    }

    /// <summary>
    /// Kiểm tra xem đã max cấp (sẵn sàng tiến hóa) chưa
    /// </summary>
    public bool IsReadyToEvolve()
    {
        return isReadyToEvolve;
    }

    /// <summary>
    /// Kiểm tra xem đã max tiến hóa (không thể nâng cấp được nữa)
    /// </summary>
    public bool IsAtMaxEvolution()
    {
        // Hỏi LevelUpManager xem còn cấp tiếp theo không
        return LevelUpManager.Instance.GetNextLevelPrefab(LevelUpType.Tower, towerNameKey, evolutionLevel) == null;
    }

    /// <summary>
    /// Hàm chính, được gọi bởi Button
    /// </summary>
    public void LevelUp()
    {
        if (isReadyToEvolve)
        {
            // 1. Đã max sub-level -> Tiến hóa
            EvolveTower();
        }
        else if (subLevel < statsList.Count)
        {
            // 2. Chưa max sub-level -> Lên cấp
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

        // (Kiểm tra tiền ở đây)
        // if (GameManager.Instance.Money < levelStats.cost) return;
        // GameManager.Instance.SpendMoney(levelStats.cost);

        // Cộng chỉ số
        stats.Ammor += levelStats.ammor;
        stats.Heath += levelStats.heath;
        stats.AttackDamage += levelStats.attackDamage;
        stats.AttackSpeed += levelStats.attackSpeed;
        stats.MoveSpeed += levelStats.moveSpeed;
        stats.AttackRange += levelStats.attackRange;
        stats.TriggerRange += levelStats.triggerRange;

        subLevel++; // Tăng cấp nội bộ
        UpdateLevelState(); // Cập nhật lại trạng thái

        // Bắn sự kiện để UIManager cập nhật (vì prefab không đổi)
        GameEvent.Instance.OnTriggerTowerLevelUp(gameObject);
    }

    /// <summary>
    /// Tiến hóa (thay prefab)
    /// </summary>
    private void EvolveTower()
    {
        GameObject nextLevelPrefab = LevelUpManager.Instance.GetNextLevelPrefab(
            LevelUpType.Tower,
            towerNameKey,
            evolutionLevel
        );

        if (nextLevelPrefab != null)
        {
            // (Kiểm tra tiền ở đây, dùng LevelUpManager để lấy cost)
            // int cost = LevelUpManager.Instance.GetLevelUpCost(towerNameKey, evolutionLevel);
            // if (GameManager.Instance.Money < cost) return;
            // GameManager.Instance.SpendMoney(cost);

            // Spawn trụ mới
            GameObject newTower = ObjectPoolManager.SpawnObject(
                nextLevelPrefab,
                transform.position,
                transform.rotation,
                ObjectPoolManager.PoolType.Tower
            );

            // Bắn sự kiện (BuildManager và UIManager sẽ bắt sự kiện này)
            GameEvent.Instance.OnTriggerTowerLevelUp(newTower);

            // Trả trụ cũ về pool
            ObjectPoolManager.ReturnObject(gameObject);
        }
        else
        {
            Debug.Log("Đã đạt cấp tiến hóa tối đa!");
        }
    }

    /// <summary>
    /// Lấy chi phí nâng cấp cho lần tiếp theo
    /// </summary>
    public int GetNextLevelCost()
    {
        if (isReadyToEvolve)
        {
            // Lấy chi phí tiến hóa
            return LevelUpManager.Instance.GetLevelUpCost(towerNameKey, evolutionLevel);
        }
        else if (subLevel < statsList.Count)
        {
            // Lấy chi phí lên cấp nội bộ
            return statsList[subLevel].cost;
        }
        return 0; // Max
    }

    /// <summary>
    /// Cập nhật lại trạng thái (gọi sau khi lên cấp)
    /// </summary>
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