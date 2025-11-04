using UnityEngine;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    [Header("Master Unit List")]
    [Tooltip("Gán TẤT CẢ các ScriptableObject UnitData của bạn vào đây")]
    public List<UnitData> allUnitData;

    private void Awake()
    {
        // Thiết lập Singleton và DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Các script khác (như SpawnUnit) sẽ gọi hàm này để lấy danh sách lính
    /// </summary>
    public List<UnitData> GetAvailableUnits()
    {
        return allUnitData;
    }

    /// <summary>
    /// (Tương lai) Bạn sẽ gọi hàm này từ menu nâng cấp để mở khóa lính
    /// </summary>
    public void UnlockUnit(string unitName)
    {
        UnitData unit = allUnitData.Find(u => u.unitName == unitName);
        if (unit != null)
        {
            unit.isUnlocked = true;
            // TODO: Thêm logic lưu vào PlayerPrefs
        }
    }
}