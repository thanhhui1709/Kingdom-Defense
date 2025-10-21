using UnityEngine;

public interface IHealthSystem 
{
    public bool HasDie();
    public void TakeDamage(float damageAmount,int ammor);
    public void Heal(float healAmount);
    public void Die();
}
