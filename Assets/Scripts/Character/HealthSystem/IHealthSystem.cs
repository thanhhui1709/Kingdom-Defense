using UnityEngine;

public interface IHealthSystem 
{
    public bool HasDie();
    public void TakeDamage(float damageAmount);
    public void Heal(float healAmount);
    public void Die();
}
