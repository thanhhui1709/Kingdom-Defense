// File: Scripts/Character/EnemyScript/PoisonDamageEffect.cs

using UnityEngine;

[RequireComponent(typeof(Stats))]
[RequireComponent(typeof(EnemyHealth))]
public class PoisonDamageEffect : MonoBehaviour
{
    // CÁC THÔNG SỐ HIỆN TẠI VÀ MẠNH NHẤT ĐANG ÁP DỤNG TRÊN KẺ ĐỊCH
    private float currentMaxDamagePerTick; // Sát thương mỗi tick cao nhất đã nhận
    private float currentMaxSlowFactor;    // Hệ số làm chậm cao nhất đã nhận (ví dụ: 0.3)

    [HideInInspector] public float damageTickRate;
    [HideInInspector] public int maxStacks;
    [HideInInspector] public Color poisonColor= new Color(0.5f, 1f, 0.5f,0.5f);
    [HideInInspector] public float duration=0.5f;

 
    private Stats stats;
    private EnemyHealth enemyHealth;
    private Renderer[] objectRenderer;
    private float originalSpeed;
    private Color[] originalColor;
    private float effectTimer;
    private float damageTimer;
    private int currentStacks = 0;

    // Đảm bảo Awake() đã lấy component và lưu originalSpeed/Color

    // ... (Hàm Awake() giữ nguyên)
    void Awake()
    {
        stats = GetComponent<Stats>();
        enemyHealth = GetComponent<EnemyHealth>();
        objectRenderer = GetComponentsInChildren<Renderer>();

        originalSpeed = stats.MoveSpeed;

        if (objectRenderer != null)
        {
            originalColor = new Color[objectRenderer.Length];
            for (int i = 0; i < objectRenderer.Length; i++)
            {
                originalColor[i] = objectRenderer[i].material.color;
            }
        }
     
    }
    // ... (Hàm Update() giữ nguyên, sử dụng currentMaxDamagePerTick)
    void Update()
    {
        // 1. Cập nhật thời gian độc
        effectTimer -= Time.deltaTime;
        if (effectTimer <= 0)
        {
            RemoveEffect();
            return;
        }

        // 2. Gây sát thương liên tục (Sử dụng currentMaxDamagePerTick)
        damageTimer -= Time.deltaTime;
        if (damageTimer <= 0)
        {
            // Sát thương nhân với số Stack và Sát thương Cao nhất đã nhận
            enemyHealth.TakeAbsoluteDamage(currentMaxDamagePerTick * currentStacks);

            damageTimer = damageTickRate;
        }
    }


    // Hàm công khai được gọi từ viên đạn
    public void ApplyStack(float newDuration, float newSlowFactor, float newDamagePerTick)
    {
        // 1. QUẢN LÝ THÔNG SỐ TỐT NHẤT

        // A. Kiểm tra Sát thương Mạnh nhất
        if (newDamagePerTick > currentMaxDamagePerTick)
        {
            currentMaxDamagePerTick = newDamagePerTick;
        }

        // B. Kiểm tra Làm chậm Mạnh nhất (Hệ số làm chậm lớn nhất = Giảm tốc độ mạnh nhất)
        if (newSlowFactor > currentMaxSlowFactor)
        {
            currentMaxSlowFactor = newSlowFactor;
            ApplyMovementSlow(); // Áp dụng làm chậm mới ngay lập tức
        }

        // C. Kiểm tra Thời gian Dài nhất (Chỉ reset nếu thời gian mới dài hơn hoặc là thời gian hết hạn)
        // Ta dùng thời gian của viên đạn mới để reset duration
        effectTimer = newDuration;
        duration=newDuration;

        // 2. CỘNG DỒN VÀ RESET

        // Tăng cộng dồn, giới hạn bởi maxStacks
        if (currentStacks < maxStacks)
        {
            currentStacks++;
        }

        // Luôn reset bộ đếm sát thương
        if (currentStacks == 0)
        {
            damageTimer = 0f;
        }

        // 3. Đổi màu vật thể (Chỉ đổi màu Stack đầu tiên)
        if (currentStacks == 1 && objectRenderer != null)
        {
            foreach (var renderer in objectRenderer)
            {
                renderer.material.color = poisonColor;
            }
        }
    }
    public void RefreshEffect()
    {
        effectTimer = duration;

    }

    // Sửa lại hàm này để sử dụng currentMaxSlowFactor
    private void ApplyMovementSlow()
    {
        // Sử dụng hệ số làm chậm cao nhất đã được lưu
        stats.MoveSpeed = originalSpeed * (1f - currentMaxSlowFactor);
    }

    // ... (Hàm RemoveEffect() giữ nguyên)
    private void RemoveEffect()
    {
        if (stats != null)
        {
            // Quan trọng: Khôi phục tốc độ ban đầu
            stats.MoveSpeed = originalSpeed;
        }
        if (objectRenderer != null)
        {
            for (int i = 0; i < objectRenderer.Length; i++)
            {
                objectRenderer[i].material.color = originalColor[i];
            }
        }
        Destroy(this);
    }
}