using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NormalSlice", menuName = "AttackBehavior/NormalSlice")]
public class NormalSlice : AttackBehavior
{
    public float criticalChance = 0.1f;
    public float damageMultiplier = 1f;
    public override void Execute(MonoBehaviour runner, Stats attackerStats, GameObject target)
    {


        var health = target.GetComponent<IHealthSystem>();


        float value = Random.Range(0, 1f);
        if (value < criticalChance)
        {
            health.TakeDamage(attackerStats.AttackDamage * 2*damageMultiplier);
        }
        else
        {
            health.TakeDamage(attackerStats.AttackDamage*damageMultiplier);
        }


    }

}

