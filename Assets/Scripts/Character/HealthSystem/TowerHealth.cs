using UnityEngine;
using System; // Cần cho "Action"
using DG.Tweening;
using System.Collections;
[RequireComponent(typeof(Stats))]
public class TowerHealth : MonoBehaviour, IHealthSystem
{
    private TowerController towerController;
    private Stats stats;
    private bool hasDie;
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    public GameObject hurtEffect;

    [Header("Death Effect")]
    [SerializeField] private float sinkAmount = 6f;
    [SerializeField] private float sinkDuration = 1.5f;
    // 1. SỰ KIỆN: Bất cứ ai quan tâm đều có thể đăng ký
    // Gửi ra (currentHealth, maxHealth)
    public event Action<float, float> OnHealthChanged;
    public AudioClip destroySound;
    // 2. Thêm "getters" để script bên ngoài có thể đọc giá trị
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    void Awake()
    {
        stats = GetComponent<Stats>();
        maxHealth = stats.Heath; // Giả sử Stats có biến Heath
        towerController = GetComponent<TowerController>();
    }

    private void OnEnable()
    {
       
        hasDie = false;
        currentHealth = maxHealth;
        // Phát sự kiện ngay khi bật để UI (nếu đang xem) cập nhật
        towerController.enabled = true;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        hurtEffect.SetActive(false);

        InvokeRepeating(nameof(HurtEffect), 0f, 0.5f);  
    }

    public void Die()
    {
        ObjectPoolManager.PlayAudio(destroySound, transform.position, 1.0f);
        // 1. Guard: Đảm bảo chỉ chết 1 lần
        if (hasDie) return;
        hasDie = true;

      
        towerController.enabled = false; // Vô hiệu hóa hành vi trụ
        if (hurtEffect != null) hurtEffect.SetActive(false);
     

        // 3. Chạy hiệu ứng chìm (DOTween)
        float targetY = transform.position.y - sinkAmount;

        transform.DOMoveY(targetY, sinkDuration)
            .SetEase(Ease.InCubic) // Bắt đầu chậm, kết thúc nhanh (cho cảm giác nặng)
            .OnComplete(() => {
               
               StartCoroutine(ReturAfterDeath());
            });
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


    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    private void HurtEffect()
    {
        // Giả sử hurtEffect là một GameObject
        if (hurtEffect == null) return;

        float healthPercentage = currentHealth / maxHealth;

        if (healthPercentage < 0.2f)
        {
            hurtEffect.SetActive(true);
            hurtEffect.transform.localScale = new Vector3(3f, 3f, 3f);
        }
        else if (healthPercentage < 0.45f)
        {
            hurtEffect.SetActive(true);
            hurtEffect.transform.localScale = new Vector3(2f, 2f, 2f);
        }
        else if (healthPercentage < 0.7f)
        {
            hurtEffect.SetActive(true);
            hurtEffect.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        }
        else
        {
            // Nếu máu trên 70%, hãy ẩn hiệu ứng đi

            hurtEffect.SetActive(false);
        }
    }
    IEnumerator ReturAfterDeath()
    {
        yield return new WaitForSeconds(sinkDuration + 10f);
        ObjectPoolManager.ReturnObject(gameObject);
    }
}