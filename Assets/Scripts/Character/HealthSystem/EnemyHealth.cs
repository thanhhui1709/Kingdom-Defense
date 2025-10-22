using UnityEngine;

[RequireComponent(typeof(Stats))]
public class EnemyHealth : MonoBehaviour, IHealthSystem
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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="damageAmount"> số damage nhận</param>
    /// <param name="ammorIgnore"> Số giáp bỏ qua</param>
    public void TakeDamage(float damageAmount,float ammorIgnore)
    {
        ammorIgnore = Mathf.Clamp(ammorIgnore, 0, stats.Ammor*1.2f);
        float takenDamage = Util.CalculateDamage(damageAmount, stats.Ammor- ammorIgnore);
        currentHealth = Mathf.Clamp(currentHealth - takenDamage, 0, maxHealth);
        Debug.Log("" + gameObject.name + " took " + takenDamage + " damage. Current Health: " + currentHealth); 
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }

    }
    /// <summary>
    /// gây sát thương mặc định không bỏ qua giáp
    public void TakeDamage(float damageAmount)
    {
        float takenDamage = Util.CalculateDamage(damageAmount, stats.Ammor);
        currentHealth = Mathf.Clamp(currentHealth - takenDamage, 0, maxHealth);
        Debug.Log("" + gameObject.name + " took " + takenDamage + " damage. Current Health: " + currentHealth); 
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }

    }
    public void TakeAbsoluteDamage(float takenDamage)
    {
  
        currentHealth = Mathf.Clamp(currentHealth - takenDamage, 0, maxHealth);
        Debug.Log("" + gameObject.name + " took " + takenDamage + " damage. Current Health: " + currentHealth);
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    public void Die()
    {
        isDead = true;
        gameObject.SetActive(false);
        GameEvent.Instance.OnTriggerEnemyDie(stats.Money);
    }
}
