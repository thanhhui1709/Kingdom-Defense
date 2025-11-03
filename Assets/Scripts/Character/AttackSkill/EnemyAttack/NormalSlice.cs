using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NormalSlice", menuName = "AttackBehavior/NormalSlice")]
public class NormalSlice : AttackBehavior
{
    public AudioClip attackSound;
    public float criticalChance = 0.1f;
    public float damageMultiplier = 1f;
    public override void Execute(MonoBehaviour runner, Stats attackerStats, GameObject target)
    {

        ObjectPoolManager.PlayAudio(attackSound, runner.transform.position,1f);
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

