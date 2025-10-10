using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class ATowerSkill : ScriptableObject
{
    public abstract void DoAttack(Transform shooter, GameObject projectile, List<GameObject> target);
}
