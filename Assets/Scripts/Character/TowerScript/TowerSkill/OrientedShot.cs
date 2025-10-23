using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Skill", menuName = "Tower Skill/OrientedShot")]
public class OrientedShot : ATowerSkill
{
    [SerializeField] private int numberOfShoot;

    [SerializeField] private Vector3 offset;
    // implement later for multi-target attack
    public override void DoAttack(Transform shooter, GameObject projectile, List<GameObject> target,float damage)
    {
        GameObject go = ObjectPoolManager.SpawnObject(projectile, shooter.position+ offset, Quaternion.identity,ObjectPoolManager.PoolType.TowerProjectile);
        IProjectile projectile1 = go.GetComponent<IProjectile>();
        if (projectile1 == null) Debug.LogError("Projectile does not implement IProjectile interface.");
        projectile1.Launch(shooter, target,damage);
    }
}
