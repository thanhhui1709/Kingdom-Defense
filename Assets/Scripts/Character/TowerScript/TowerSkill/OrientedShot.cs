using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Skill", menuName = "Tower Skill/OrientedShot")]
public class OrientedShot : ATowerSkill
{
    [SerializeField] private int numberOfShoot;
   
    // implement later for multi-target attack
    public override void DoAttack(Transform shooter, GameObject projectile, List<GameObject> target)
    {
        GameObject go = Instantiate(projectile, shooter.position, Quaternion.identity);
        IProjectile projectile1 = go.GetComponent<IProjectile>();
        if (projectile1 == null) Debug.LogError("Projectile does not implement IProjectile interface.");
        projectile1.Launch(shooter, target);
    }
}
