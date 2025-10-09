using UnityEngine;
using UnityEngine.UI;

[System.Serializable] // Cho phép hiển thị trong Inspector
public class Stats
{
    public float maxHealth = 100f;       // Máu tối đa
    public float currentHealth;          // Máu hiện tại
    public float moveSpeed = 5f;         // Tốc độ di chuyển (cho lính)
    public float attackRange = 10f;      // Tầm đánh/tầm bắn
    public float attackSpeed = 1f;       // Tốc độ đánh (giây giữa các lần tấn công)
    public float damage = 20f;           // Sát thương gây ra

    [Header("UI")]
    public Slider healthBar;             // Thanh máu (UI Slider)

    // Constructor để khởi tạo
    public Stats()
    {
        currentHealth = maxHealth;
    }

    // Phương thức cập nhật thanh máu
    public void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
    }

    // Phương thức nhận sát thương
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }
}