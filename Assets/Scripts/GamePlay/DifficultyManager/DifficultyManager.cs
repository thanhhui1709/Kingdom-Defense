using UnityEngine;
using System.Collections.Generic;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    [Header("Global Settings")]
    [SerializeField] private bool useTimeScaling = true;
    [SerializeField] private float buffPerMinute = 0.05f;
    [SerializeField] private bool useWaveScaling = true;
    [SerializeField] private float buffPerWave = 0.05f;

    // List cấu hình riêng (bạn đã có)
    [SerializeField] private List<PrefabBuff> specificBuffs;

    private float startTime;
    private int currentWaveCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        startTime = Time.time;
    }

    public void IncrementWaveCount()
    {
        currentWaveCount++;
    }

    /// <summary>
    /// Hàm này trả về gói Buff đầy đủ (Health, Damage, Money)
    /// </summary>
    public BuffData GetBuffData(string enemyName)
    {
        // 1. Tính hệ số chung (Global)
        float globalMult = 1.0f;

        if (useTimeScaling)
        {
            float minutesPassed = (Time.time - startTime) / 60f;
            globalMult += minutesPassed * buffPerMinute;
        }

        if (useWaveScaling)
        {
            globalMult += currentWaveCount * buffPerWave;
        }

        // Mặc định áp dụng global cho cả máu và dame
        float finalHealthMult = globalMult;
        float finalDamageMult = globalMult;
        float finalMoneyMult = globalMult; // Tiền cũng tăng theo độ khó chung

        // 2. Tìm cấu hình riêng cho từng con (Specific)
        foreach (var buff in specificBuffs)
        {
            // So sánh tên (Dùng Contains để đỡ phải gõ chính xác 100% tên Clone)
            if (enemyName.Contains(buff.prefabName))
            {
                finalHealthMult *= buff.healthMultiplier;
                finalDamageMult *= buff.damageMultiplier;
                // Nếu muốn buff riêng tiền thì thêm vào class PrefabBuff
                break;
            }
        }

        return new BuffData(finalHealthMult, finalDamageMult, finalMoneyMult);
    }
}

// Class cấu hình của bạn (Giữ nguyên)
[System.Serializable]
public class PrefabBuff
{
    public string prefabName;
    public float healthMultiplier = 1.0f;
    public float damageMultiplier = 1.0f;
}

// Struct này dùng để chứa kết quả tính toán buff
public struct BuffData
{
    public float HealthMultiplier;
    public float DamageMultiplier;
    public float MoneyMultiplier;

    // Constructor tiện lợi
    public BuffData(float health, float damage, float money)
    {
        HealthMultiplier = health;
        DamageMultiplier = damage;
        MoneyMultiplier = money;
    }
}