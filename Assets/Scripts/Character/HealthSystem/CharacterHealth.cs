using UnityEngine;

[RequireComponent(typeof(Stats))]
public class CharacterHealth : MonoBehaviour, IHealthSystem
{
    private Stats stats;
    private bool isDead = false;


    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth;
    private void Start()
    {
        stats = GetComponent<Stats>();
        maxHealth = stats.Heath;
        currentHealth = maxHealth;
    }
    private void OnEnable()
    {
        isDead = false;
        currentHealth = maxHealth;
    }
    private void Update()
    {

    }
    public bool HasDie() => isDead;


    public void Heal(float healAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);
    }

    public void TakeDamage(float damageAmount, int ammor)
    {
        float takenDamage = Util.CalculateDamage(damageAmount, ammor);
        currentHealth = Mathf.Clamp(currentHealth - takenDamage, 0, maxHealth);

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }

    }

    public void Die()
    {
        isDead = true;
        gameObject.SetActive(false);
    }
}
