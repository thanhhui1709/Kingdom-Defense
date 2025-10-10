using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Skill", menuName = "Tower Skill/OrientedShot")]
public class OrientedShot : ATowerSkill
{
    [SerializeField] private int numberOfShoot;
    public override void DoAttack(Transform shooter, GameObject projectile, GameObject target)
    {
      GameObject go=Instantiate (projectile, shooter.position, Quaternion.identity);
      go.TryGetComponent<OrientedArrow>(out OrientedArrow orientedArrow);
      orientedArrow.SetTarget(target);
    }
    // implement later for multi-target attack
    public override void DoAttack(Transform shooter, GameObject projectile, List<GameObject> target)
    {
        throw new System.NotImplementedException();
    }
}
