using UnityEngine;
using System; // Cần cho "Action"

[RequireComponent(typeof(Stats))]
public class TowerHealth : MonoBehaviour, IHealthSystem
{
    private Stats stats;
    private bool hasDie;
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;

    // 1. SỰ KIỆN: Bất cứ ai quan tâm đều có thể đăng ký
    // Gửi ra (currentHealth, maxHealth)
    public event Action<float, float> OnHealthChanged;

    // 2. Thêm "getters" để script bên ngoài có thể đọc giá trị
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    // XÓA: Toàn bộ biến UI đã bị xóa
    // public Slider healthBar;
    // public Image healthFill;
    // public Gradient gradient;

    void Awake()
    {
        stats = GetComponent<Stats>();
        maxHealth = stats.Heath; // Giả sử Stats có biến Heath
    }

    private void OnEnable()
    {
        hasDie = false;
        currentHealth = maxHealth;
        // Phát sự kiện ngay khi bật để UI (nếu đang xem) cập nhật
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Die()
    {
        hasDie = true;
        // TODO: Thêm logic khi trụ bị phá hủy
    }

    public bool HasDie()
    {
        return hasDie;
    }

    public void Heal(float healAmount)
    {
        if (hasDie) return;
        currentHealth = Mathf.Clamp(healAmount + currentHealth, 0, maxHealth);

        // 3. Phát sự kiện
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damageAmount)
    {
        if (hasDie) return;

        float damageTaken = Util.CalculateDamage(damageAmount, stats.Ammor);
        currentHealth = Mathf.Clamp(currentHealth - damageTaken, 0, maxHealth);

        // 4. Phát sự kiện
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // XÓA: Hàm này không còn cần thiết
    // private void UpdateHeathBar() { ... }

    // (Bạn thiếu hàm SetHealth trong IHealthSystem, nhưng tôi sẽ bỏ qua)
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}