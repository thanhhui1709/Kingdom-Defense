using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Skill", menuName = "Tower Skill/ContinuousLightningBeam")]
public class ContinuousLightningBeam : ATowerSkill
{
    // Chúng ta không cần biến gì ở đây vì logic sẽ nằm trong prefab

    // Giả sử hàm DoAttack gốc của bạn là đây
    public override void DoAttack(Transform shooter, GameObject projectilePrefab, List<GameObject> targetsInRange,float damage)
    {
        if (targetsInRange.Count == 0) return;

        // 1. Tìm mục tiêu chính (ví dụ: con gần nhất)
        GameObject primaryTarget = targetsInRange
            .OrderBy(e => Vector3.Distance(shooter.position, e.transform.position))
            .FirstOrDefault();

        if (primaryTarget == null) return;

        // 2. Lấy thông số sát thương từ Stats của trụ
        Stats towerStats = shooter.GetComponentInParent<Stats>();
        if (towerStats == null)
        {
            Debug.LogError("Trụ không có component Stats!");
            return;
        }

        // 3. Tạo ra prefab tia điện
        GameObject beamGO = ObjectPoolManager.SpawnObject(projectilePrefab, shooter.position, Quaternion.identity,ObjectPoolManager.PoolType.TowerProjectile);

        // 4. Lấy script logic từ tia điện và "khởi động" nó
        LightningBeam beamScript = beamGO.GetComponent<LightningBeam>();
        if (beamScript != null)
        {
            // Ra lệnh cho tia điện: "Bám theo trụ này, tấn công mục tiêu này, với sát thương này"
            beamScript.Launch(shooter, targetsInRange, towerStats.AttackDamage);
        }
        else
        {
            Debug.LogError("Prefab tia điện thiếu script LightningBeamProjectile!");
            Destroy(beamGO);
        }
    }

  
}