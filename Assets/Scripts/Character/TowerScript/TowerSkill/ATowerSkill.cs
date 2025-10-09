using UnityEngine;

public abstract class ATowerSkill : ScriptableObject
{
    public abstract void DoAttack(Transform shooter, GameObject projectile, GameObject target);
}
