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
    public override void Execute(MonoBehaviour runner, Stats attackerStats, GameObject target)
    {
        if (projectile == null)
        {
            Debug.Log("No projectile assigned for RangeAttack.");
            return;
        }
        runner.StartCoroutine(SpawnProjectile(runner, attackerStats, target));



    }
    IEnumerator SpawnProjectile(MonoBehaviour runner, Stats attackerStats, GameObject target)
    {
        for (int i = 0; i < numberOfShoot; i++)
        {

            GameObject project = ObjectPoolManager.SpawnObject(projectile, runner.transform.position + offsetPos, Quaternion.identity * offsetRotation, ObjectPoolManager.PoolType.EnemyProjectile);

            IProjectile prj = project.GetComponent<IProjectile>();
            if (prj != null)
            {
                prj.Launch(new List<GameObject> { target }, attackerStats.AttackDamage*damageMultiplier);
            }
        }
        yield return new WaitForSeconds(1f / fireRate);
    }
}
