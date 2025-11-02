using System.Collections;
using System.Collections.Generic;
using System.Linq; // Cần cho FindAll
using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Skill", menuName = "Tower Skill/OrientedShot")]
public class OrientedShot : ATowerSkill
{
    [SerializeField] private int numberOfShoot = 1;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float timeBetweenShots = 0.1f;

    // XÓA BỎ BIẾN XOAY 180 ĐỘ (rotationFix)

    public override void DoAttack(MonoBehaviour runner, Transform shooter, GameObject projectile, List<GameObject> targets, float damage)
    {
        runner.StartCoroutine(FireSequence(runner, shooter, projectile, targets, damage));
    }

    private IEnumerator FireSequence(MonoBehaviour runner, Transform shooter, GameObject projectilePrefab, List<GameObject> targets, float damage)
    {
        for (int i = 0; i < numberOfShoot; i++)
        {
            List<GameObject> validTargets = targets.FindAll(t => t != null && t.activeInHierarchy);
            if (validTargets.Count == 0)
            {
                yield break;
            }

            // Tính toán vị trí và hướng xoay GỐC của nòng súng
            Vector3 spawnPos = shooter.position + (shooter.rotation * offset);
            Quaternion spawnRot = shooter.rotation;

            // Spawn đạn
            GameObject go = ObjectPoolManager.SpawnObject(
                projectilePrefab,
                spawnPos,
                spawnRot, // Dùng hướng xoay GỐC (mũi tên sẽ tự sửa)
                ObjectPoolManager.PoolType.TowerProjectile
            );

            // Launch
            IProjectile projectile1 = go.GetComponent<IProjectile>();
            if (projectile1 == null)
            {
                Debug.LogError("Projectile does not implement IProjectile interface.");
                ObjectPoolManager.ReturnObject(go);
                yield break;
            }

            projectile1.Launch(validTargets, damage);

            if (i < numberOfShoot - 1)
            {
                yield return new WaitForSeconds(timeBetweenShots);
            }
        }
    }
}