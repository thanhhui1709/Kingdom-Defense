using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cannon Shot", menuName = "Tower Skill/CannonShot")]
public class CannonShot : ATowerSkill
{
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float aimHeightOffset = 0.5f;
    [SerializeField] private float minRange = 4f;
    [SerializeField] private float muzzleOffset = 4f;
    [SerializeField] private float projectileLifeTime = 6f;

    public override void DoAttack(Transform shooter, GameObject projectilePrefab, List<GameObject> targets, float damage)
    {
        if (targets == null || targets.Count == 0 || projectilePrefab == null)
            return;

        GameObject target = targets.FirstOrDefault();
        if (target == null) return;

        // --- Quay tháp về phía mục tiêu ---
        Vector3 flatDir = target.transform.position - shooter.position;
        flatDir.y = 0f;

        if (flatDir.sqrMagnitude > 0.001f)
            shooter.rotation = Quaternion.LookRotation(flatDir.normalized, Vector3.up);

        // --- Kiểm tra tầm bắn ---
        float distance = Vector3.Distance(shooter.position, target.transform.position);
        if (distance < minRange)
            return;

        // --- Xác định vị trí sinh viên đạn ---
        Transform barrel = shooter.Find("Cannon_2");
        Vector3 spawnPos;
        Quaternion spawnRot;

        if (barrel != null)
        {
           
            spawnPos = barrel.position + barrel.up * muzzleOffset;
            spawnRot = barrel.rotation;
        }
        else
        {
            spawnPos = shooter.position + shooter.up * muzzleOffset;
            spawnRot = shooter.rotation;
        }

        
        Debug.DrawLine(barrel != null ? barrel.position : shooter.position, spawnPos, Color.red, 2f);

       
        GameObject projectile = ObjectPoolManager.SpawnObject(projectilePrefab, spawnPos, spawnRot,ObjectPoolManager.PoolType.TowerProjectile);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("CannonShot: projectilePrefab cần có Rigidbody!");
            Object.Destroy(projectile);
            return;
        }
        Stats shooterStats=shooter.GetComponentInParent<Stats>();
        IProjectile projectile1 = projectile.GetComponent<IProjectile>();
        if (projectile1 != null && shooterStats!=null)
        {
            projectile1.Launch(barrel,targets,shooterStats.AttackDamage);
        }

        
        Vector3 aimTarget = target.transform.position + Vector3.up * aimHeightOffset;
        Vector3 shootDir = (aimTarget - spawnPos).normalized;

        rb.linearVelocity = shootDir * projectileSpeed;
        projectile.transform.forward = shootDir;

        Object.Destroy(projectile, projectileLifeTime);
    }
}
