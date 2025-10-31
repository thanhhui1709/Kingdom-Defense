using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Stats), typeof(Collider))] // Cần Collider để hover
public class EnemyHealth : MonoBehaviour, IHealthSystem
{
    private Stats stats;
    private bool isDead = false;

    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth;

    [Header("Health Bar UI")]
    [Tooltip("Canvas cha chứa thanh máu")]
    [SerializeField] private GameObject healthBarCanvas;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthFill;
    [SerializeField] private Gradient gradient;

    [Header("UI Visibility")]
    [Tooltip("Số giây thanh máu hiển thị sau khi bị đánh")]
    [SerializeField] private float showDuration = 3f;

    private AnimationController anim;
    private float showTimer; // Bộ đếm lùi
    private bool isHovering = false; // Chuột có đang hover không
    private Camera mainCamera;

    private void Awake()
    {
        stats = GetComponent<Stats>();
        anim = GetComponent<AnimationController>();
        mainCamera = Camera.main; // Lấy camera chính
        maxHealth = stats.Heath;
        currentHealth = maxHealth;
    }
 

    private void OnEnable()
    {
       
        isDead = false;
        currentHealth = maxHealth; // Đặt lại đầy máu

        // Reset trạng thái UI
        showTimer = 0f;
        isHovering = false;
        // Ẩn thanh máu lúc bắt đầu và cập nhật
        if (healthBarCanvas != null)
            healthBarCanvas.SetActive(false);
        UpdateHealthBar();
        anim.Play(AnimationType.Die, false);
    }

    private void Update()
    {
        // Chỉ chạy đếm lùi nếu timer > 0
        if (showTimer > 0f)
        {
            showTimer -= Time.deltaTime;

            // Nếu hết giờ và chuột không hover, thì mới ẩn
            if (showTimer <= 0f && !isHovering)
            {
                HideHealthBar();
            }
        }
    }

    private void LateUpdate()
    {
        // Xoay thanh máu (nếu đang bật) về phía camera
        if (healthBarCanvas != null && healthBarCanvas.activeInHierarchy)
        {
            healthBarCanvas.transform.rotation = mainCamera.transform.rotation;
        }
    }

    // --- Logic Hiển thị/Ẩn UI ---

    private void ShowHealthBar()
    {
        if (healthBarCanvas != null && !isDead)
            healthBarCanvas.SetActive(true);
    }

    private void HideHealthBar()
    {
        if (healthBarCanvas != null)
            healthBarCanvas.SetActive(false);
    }

    // --- Event xử lý Hover chuột ---

    private void OnMouseEnter()
    {
        isHovering = true;
        ShowHealthBar();
    }

    private void OnMouseExit()
    {
        isHovering = false;

        // Chỉ ẩn nếu timer cũng đã hết
        if (showTimer <= 0f)
        {
            HideHealthBar();
        }
    }

    // --- Logic Cập nhật Thanh máu ---

    private void UpdateHealthBar()
    {
        if (healthBar == null) return;

        float fillAmount = 0f;
        if (maxHealth > 0) // Tránh lỗi chia cho 0
        {
            fillAmount = currentHealth / maxHealth;
        }

        healthBar.value = fillAmount;

        if (healthFill != null && gradient != null)
        {
            healthFill.color = gradient.Evaluate(fillAmount);
        }
    }

    // --- Logic Máu (Đã sửa) ---

    public bool HasDie() => isDead;

    public void Heal(float healAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);
        UpdateHealthBar(); // Cập nhật UI khi hồi máu
    }

    public void TakeDamage(float damageAmount, float ammorIgnore)
    {
        if (isDead) return;

        ammorIgnore = Mathf.Clamp(ammorIgnore, 0, stats.Ammor * 1.2f);
        float takenDamage = Util.CalculateDamage(damageAmount, stats.Ammor - ammorIgnore);
        currentHealth = Mathf.Clamp(currentHealth - takenDamage, 0, maxHealth);

        // Kích hoạt UI
        showTimer = showDuration;
        ShowHealthBar();
        UpdateHealthBar();

        if (currentHealth <= 0) Die();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        float takenDamage = Util.CalculateDamage(damageAmount, stats.Ammor);
        currentHealth = Mathf.Clamp(currentHealth - takenDamage, 0, maxHealth);

        // Kích hoạt UI
        showTimer = showDuration;
        ShowHealthBar();
        UpdateHealthBar();

        if (currentHealth <= 0) Die();
    }

    public void TakeAbsoluteDamage(float takenDamage)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - takenDamage, 0, maxHealth);

        // Kích hoạt UI
        showTimer = showDuration;
        ShowHealthBar();
        UpdateHealthBar();

        if (currentHealth <= 0) Die();
    }

    public void Die()
    {
        if (isDead) return; // Đảm bảo Die() chỉ chạy 1 lần

        isDead = true;

        // Tắt UI trước khi tắt object
        if (healthBarCanvas != null)
            healthBarCanvas.SetActive(false);

        anim.Play(AnimationType.Die, true);
        GameEvent.Instance.OnTriggerEnemyDie(stats.Money);
        StartCoroutine(DisableAfterTime(5f)); // Chờ 2 giây trước khi tắt
    }
    IEnumerator DisableAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPoolManager.ReturnObject(gameObject);
    }
}