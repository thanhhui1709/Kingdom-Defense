using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static Unity.Collections.Unicode;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "RangeAttack", menuName = "AttackBehavior/RangeAttack")]
public class RangeAttack : AttackBehavior
{
    public int numberOfShoot;
    public float fireRate;
    public GameObject projectile;
    public Vector3 offsetPos;
    public Quaternion offsetRotation;
    public float damageMultiplier = 1f;

    private int count = 0;
    public override void Execute(MonoBehaviour runner, Stats attackerStats, GameObject target)
    {
        if (projectile == null)
        {
            Debug.Log("No projectile assigned for RangeAttack.");
            return;
        }

        runner.StartCoroutine(ShootRoutine(runner, attackerStats, target));


    }
    private IEnumerator ShootRoutine(MonoBehaviour runner, Stats attackerStats, GameObject target)
    {
        while ((target != null && target.activeInHierarchy) && count < numberOfShoot) // vẫn bắn khi mục tiêu còn sống
        {
            SpawnProjectile(runner, attackerStats, target);

            yield return new WaitForSeconds(1f / fireRate);
        }
    }
    private void SpawnProjectile(MonoBehaviour runner, Stats attackerStats, GameObject target)
    {


        Vector3 spawnPos = runner.transform.position + (runner.transform.rotation * offsetPos);

        GameObject proj = ObjectPoolManager.SpawnObject(
            projectile,
            spawnPos,
            Quaternion.identity,
            ObjectPoolManager.PoolType.EnemyProjectile
        );

        Vector3 dir = (target.transform.position - proj.transform.position).normalized;
        proj.transform.right = dir;

        if (offsetRotation != Quaternion.identity)
            proj.transform.rotation *= offsetRotation;

        if (proj.TryGetComponent<IProjectile>(out var prj))
        {
            prj.Launch(new List<GameObject> { target }, attackerStats.AttackDamage * damageMultiplier);
        }
        count++;

    }


}
