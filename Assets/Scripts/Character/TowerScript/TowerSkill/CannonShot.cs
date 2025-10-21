using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cannon Shot", menuName = "Tower Skill/CannonShot")]
public class CannonShot : ATowerSkill
{
    public float shootForce = 500f;
    public float fireRate = 1f;
    public float range = 10f;

    // Giữ nguyên biến offset, bạn có thể phải dùng giá trị lớn hơn 20f
    public float muzzleOffsetDistance = 1.5f;

    public override void DoAttack(Transform shooter, GameObject projectile, List<GameObject> targets)
    {
        if (targets == null || targets.Count == 0 || projectile == null) return;

        GameObject target = targets[0];
        if (target == null) return;

        // --- Tìm Cannon_1 (Đối tượng được quay) ---
        Transform cannon1 = shooter.GetComponentsInChildren<Transform>(true)
                                       .FirstOrDefault(t => t.name == "Cannon_1");
        if (cannon1 == null)
        {
            Debug.LogError("Không tìm thấy Cannon_1!");
            return;
        }

        // --- Tìm Cannon_2 (Đối tượng mà bạn muốn lấy vị trí, dù nó ở tâm) ---
        Transform cannon2 = shooter.GetComponentsInChildren<Transform>(true)
                                       .FirstOrDefault(t => t.name == "Cannon_2");
        if (cannon2 == null)
        {
            Debug.LogError("Không tìm thấy Cannon_2!");
            return;
        }

      
        Vector3 directionToTarget = (target.transform.position - cannon1.position).normalized;
        if (directionToTarget != Vector3.zero)
        {
            cannon1.rotation = Quaternion.LookRotation(directionToTarget);
        }

      
        Vector3 fireDirection = cannon1.forward;

        Vector3 basePos = cannon2.position;

        Vector3 spawnPos = basePos + fireDirection * muzzleOffsetDistance;

      
        Quaternion spawnRot = cannon1.rotation;

       
        GameObject bullet = GameObject.Instantiate(projectile, spawnPos, spawnRot);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
           
            rb.AddForce(fireDirection * shootForce);
        }
    }
}