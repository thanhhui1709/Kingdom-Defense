// File: Scripts/TowerScript/TowerSkill/PoisonShot.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Poison Shot", menuName = "Tower Skill/PoisonShot")]
public class PoisonShot : ATowerSkill
{
    [SerializeField]
    private GameObject poisonProjectilePrefab;

    [SerializeField]
    private Vector3 offset;


    // Chữ ký hàm này đã đúng, nó nhận vào một List<GameObject>
    public override void DoAttack(Transform shooter, GameObject projectile, List<GameObject> targets, float damage)
    {
        // Kiểm tra điều kiện cơ bản
        if (poisonProjectilePrefab == null || targets == null || targets.Count == 0)
        {
            return;
        }

        // Tạo ra viên đạn
        GameObject go = Instantiate(poisonProjectilePrefab, shooter.position + offset, shooter.rotation);

        IProjectile projectileLogic = go.GetComponent<IProjectile>();
        if (projectileLogic != null)
        {
            // === SỬA LỖI Ở ĐÂY ===
            // Trước đây: 
            // GameObject target = targets[0];
            // projectileLogic.Launch(shooter, target); // Lỗi! vì 'target' là một GameObject đơn lẻ

            // Bây giờ:  
            // Truyền trực tiếp cả danh sách 'targets' vào hàm Launch.
            // Script của viên đạn (PoisonSlowProjectile) sẽ tự xử lý việc chọn mục tiêu đầu tiên từ danh sách này.
            projectileLogic.Launch(shooter, targets, damage);
        }
        else
        {
            Debug.LogError("Prefab đạn không có script triển khai IProjectile.");
        }
    }
}