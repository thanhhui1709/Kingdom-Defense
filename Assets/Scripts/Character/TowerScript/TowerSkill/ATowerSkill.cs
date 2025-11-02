using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class ATowerSkill : ScriptableObject
{
    public abstract void DoAttack(MonoBehaviour runner,Transform shooter, GameObject projectile, List<GameObject> target,float damage);
}
