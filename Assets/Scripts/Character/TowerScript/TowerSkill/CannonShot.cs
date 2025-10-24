using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cannon Shot", menuName = "Tower Skill/CannonShot")]
public class CannonShot : ATowerSkill
{
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float aimHeightOffset = 0.5f;

    [Header("Giới hạn tầm bắn")]
    [SerializeField] private float minRange = 2f;
    [SerializeField] private float muzzleOffset = 2f;

    public override void DoAttack(Transform shooter, GameObject projectilePrefab, List<GameObject> targets, float damage)
    {
        if (targets == null || targets.Count == 0 || projectilePrefab == null)
            return;

        GameObject target = targets.FirstOrDefault();
        if (target == null) return;

        
        float distance = Vector3.Distance(shooter.position, target.transform.position);
        if (distance < minRange)
            return;

       
        Vector3 flatDir = target.transform.position - shooter.position;
        flatDir.y = 0f;
        if (flatDir.sqrMagnitude > 0.001f)
            shooter.rotation = Quaternion.LookRotation(flatDir.normalized, Vector3.up);

     
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

      
        GameObject projectile = Object.Instantiate(projectilePrefab, spawnPos, spawnRot);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("CannonShot: projectilePrefab cần có Rigidbody!");
            Object.Destroy(projectile);
            return;
        }

       
        Vector3 aimTarget = target.transform.position + Vector3.up * aimHeightOffset;
        Vector3 shootDir = (aimTarget - spawnPos).normalized;

        rb.linearVelocity = shootDir * projectileSpeed;
        projectile.transform.forward = shootDir;

       
        Object.Destroy(projectile, 6f);
    }
}
