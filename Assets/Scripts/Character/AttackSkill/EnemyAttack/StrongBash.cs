using UnityEngine;

[CreateAssetMenu(fileName = "StrongBash", menuName = "AttackBehavior/StrongBash")]
public class StrongBash : AttackBehavior
{
    public AudioClip attackSound;
    [Range(0,1)]
    public float criticalChance = 0.8f;
    public float radius = 2f;
    public LayerMask layer;
    public float damageMultiplier = 1f;
    public override void Execute(MonoBehaviour runner, Stats attackerStats, GameObject target)
    {
        ObjectPoolManager.PlayAudio(attackSound, runner.transform.position, 1f);
        Collider[] hitColliders = Physics.OverlapSphere(target.transform.position, radius,layer);
        foreach (var hitCollider in hitColliders)
        {
            IHealthSystem health = hitCollider.GetComponentInParent<IHealthSystem>();
            if (health != null)
            {
                float damage = attackerStats.AttackDamage;
                if (Random.value < criticalChance)
                {
                    damage *= 2; // Critical hit
                }
                health.TakeDamage(damage * damageMultiplier);
            }
        }

    }
}
