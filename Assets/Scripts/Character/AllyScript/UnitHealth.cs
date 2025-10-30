using UnityEngine;
using UnityEngine.UI;

public class UnitHealth : MonoBehaviour, IHealthSystem
{
    private Stats stats;
    [SerializeField]
    private float currentHealth;
    [SerializeField]
    private float maxHealth;

    [Header("UI Elements")]
    public Slider healthBar;
    public Image fill;
    public Gradient gr; 

    private Canvas healthBarCanvas; 
    private bool hasDie;

    // Awake được gọi trước OnEnable, lý tưởng để lấy component
    void Awake()
    {
        stats = GetComponent<Stats>();
        if (healthBar != null)
        {
            // Lấy Canvas cha của thanh máu để xoay nó
            healthBarCanvas = healthBar.GetComponentInParent<Canvas>();
        }
    }

    // OnEnable được gọi mỗi khi object được lấy ra từ Pool
    // Đây là nơi hoàn hảo để reset máu
    private void OnEnable()
    {
        hasDie = false;

     
        maxHealth = stats.Heath;
        currentHealth = maxHealth;

        // Cập nhật thanh máu về trạng thái đầy
        UpdateHealthBar();

        HideHealthBar();
    }

    // LateUpdate được gọi sau Update, lý tưởng để xử lý UI camera
    void LateUpdate()
    {

       
        if (healthBarCanvas != null && healthBarCanvas.gameObject.activeInHierarchy)
        {
            healthBarCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    /// <summary>
    /// Hàm trung tâm để cập nhật Slider và Màu sắc
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthBar == null) return;

        // 1. Tính toán tỉ lệ máu (0.0 -> 1.0)
        float fillAmount = currentHealth / maxHealth;

        // 2. Cập nhật giá trị của Slider
        healthBar.value = fillAmount;

        // 3. Cập nhật màu của thanh máu dựa trên Gradient
        fill.color = gr.Evaluate(fillAmount);
    }

    public void Die()
    {
        hasDie = true;

        HideHealthBar(); // Ẩn khi chết
        ObjectPoolManager.ReturnObject(gameObject);

       
    }

    public bool HasDie()
    {
        return hasDie;
    }

    public void Heal(float healAmount)
    {
        if (hasDie) return; // Không thể hồi máu cho đối tượng đã chết

        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);

        // Cập nhật lại thanh máu sau khi hồi
        UpdateHealthBar();
    }

    public void TakeDamage(float damageAmount)
    {
        if (hasDie) return;

        // Giả sử bạn có class Util và Stats có 'Ammor'
        float takenDamage = Util.CalculateDamage(damageAmount, stats.Ammor);
        currentHealth = Mathf.Clamp(currentHealth - takenDamage, 0, maxHealth);

        // Cập nhật lại thanh máu sau khi nhận sát thương
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);

        // Cập nhật lại thanh máu
        UpdateHealthBar();

        // Kiểm tra nếu set máu về 0
        if (currentHealth <= 0 && !hasDie)
        {
            Die();
        }
    }
    public void ShowHealthBar()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Ẩn thanh máu (được gọi bởi Unit.cs)
    /// </summary>
    public void HideHealthBar()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(false);
        }
    }
}