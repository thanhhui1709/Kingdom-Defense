using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class ATowerSkill : ScriptableObject
{
    //for single 
    public abstract void DoAttack(Transform shooter, GameObject projectile, GameObject target);
    // for multiple target
    public abstract void DoAttack(Transform shooter, GameObject projectile, List<GameObject> target);
}
