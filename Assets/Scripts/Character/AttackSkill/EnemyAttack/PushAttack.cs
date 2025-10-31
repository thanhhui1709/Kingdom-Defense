using UnityEngine;
[CreateAssetMenu(fileName = "PushAttack", menuName = "AttackBehavior/PushAttack")]
public class PushAttack : AttackBehavior
{
    public float pushForce = 10f;
    public override void Execute(MonoBehaviour runner, Stats attackerStats, GameObject target)
    {
        IHealthSystem health = target.GetComponent<IHealthSystem>();
        if (health != null) 
        {
            health.TakeDamage(attackerStats.AttackDamage);
            Rigidbody targetRigidbody = target.GetComponent<Rigidbody>();
            if (targetRigidbody != null)
            {
                Vector3 pushDirection = (target.transform.position - runner.transform.position).normalized;
                targetRigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse);
            }
        }
    }
}
