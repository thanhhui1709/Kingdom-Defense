using UnityEngine;
using UnityEngine.UI;

public class CastleHealth : MonoBehaviour, IHealthSystem
{
    [SerializeField]
    private float maxHealth = 1000f;
    [SerializeField]
    private float currentHealth;

    
    public Slider healthBar;
    public Image healthFill;
    public Gradient gradient;

    private bool hasDie = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hasDie = false;
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Die()
    {
        hasDie = true;
        GameEvent.Instance.OnTriggerGameOver();
    }

    public bool HasDie()
    {
        return hasDie;
    }

    public void Heal(float healAmount)
    {
        currentHealth=Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);    
        UpdateHealthBar();
    }

    public void TakeDamage(float damageAmount)
    {
       currentHealth=Mathf.Clamp(currentHealth - damageAmount, 0, maxHealth);   
       UpdateHealthBar();
        if(currentHealth <= 0)
        {
            Die();
        }
    }
    private void UpdateHealthBar()
    {
        healthBar.value = currentHealth / maxHealth;
        healthFill.color = gradient.Evaluate(healthBar.normalizedValue);
    }
}

