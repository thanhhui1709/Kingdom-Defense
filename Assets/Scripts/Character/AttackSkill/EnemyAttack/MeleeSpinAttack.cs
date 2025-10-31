using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeSpinAttack", menuName = "AttackBehavior/MeleeSpinAttack")]
public class MeleeSpinAttack : AttackBehavior
{
    [SerializeField] private float radius;
    [Range(0, 1)] public float criticalChance;
    public LayerMask layer;
    public override void Execute(MonoBehaviour runner, Stats attackerStats, GameObject target)
    {
        Collider[] hitColliders = Physics.OverlapSphere(runner.transform.position, radius, layer);
        float value = Random.Range(0, criticalChance);
        foreach (var hitCollider in hitColliders)
        {

            IHealthSystem health = hitCollider.GetComponentInParent<IHealthSystem>();
            if (health != null)
            {
                if (value <= criticalChance)
                {
                    health.TakeDamage(attackerStats.AttackDamage * 2);
                }
                else
                {
                    health.TakeDamage(attackerStats.AttackDamage);
                }
            }
            else
            {
                Debug.Log("No IHealthSystem found on " + hitCollider.gameObject.name);  
            }
        }

    }
}
