using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Cần cho FirstOrDefault

[CreateAssetMenu(fileName = "New Burst Cannon Shot", menuName = "Tower Skill/BurstCannonShot")]
public class BurstCannonShot : CannonShot
{
    [Header("Burst Settings")]
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float timeBetweenShots = 0.2f;

    [Header("Spawn Settings")]
    [Tooltip("Vị trí lệch so với 'shooter' (nòng súng)")]
    [SerializeField] private Vector3 spawnOffset;

    public override void DoAttack(MonoBehaviour runner, Transform shooter, GameObject projectilePrefab, List<GameObject> targets, float damage)
    {
        runner.StartCoroutine(BurstFire(runner, shooter, projectilePrefab, targets, damage));
    }

    /// <summary>
    /// Coroutine bắn liên thanh (ĐÃ SỬA)
    /// </summary>
    private IEnumerator BurstFire(MonoBehaviour runner, Transform shooter, GameObject projectilePrefab, List<GameObject> targets, float damage)
    {
        Transform towerBase = shooter; // 'runner' là bệ tháp

        for (int i = 0; i < burstCount; i++)
        {
            // 1. Kiểm tra và lấy mục tiêu hợp lệ ĐẦU TIÊN
            GameObject currentTarget = targets.FirstOrDefault(t => t != null && t.activeInHierarchy);
            if (currentTarget == null)
            {
                yield break; // Không còn mục tiêu, dừng bắn
            }

            // --- THÊM LOGIC XOAY (Từ CannonShot) ---

            // 2. Quay Bệ (Y-axis) - 'runner' là bệ tháp
            Vector3 flatDir = currentTarget.transform.position - towerBase.position;
            flatDir.y = 0;
            if (flatDir.sqrMagnitude > 0.001f)
                towerBase.rotation = Quaternion.LookRotation(flatDir.normalized, Vector3.up);

            // 3. Quay Nòng (X-axis) - 'shooter' là nòng súng
            // (Truy cập các biến từ lớp 'CannonShot' cha)
            Vector3 aimTargetPos = currentTarget.transform.position + Vector3.up * base.aimHeightOffset;
            Vector3 shootDir = (aimTargetPos - shooter.position).normalized;

            shooter.rotation = Quaternion.LookRotation(shootDir);

            // --- KẾT THÚC LOGIC XOAY ---

            // 4. Tính toán vị trí spawn (DỰA TRÊN HƯỚNG XOAY MỚI)
            Vector3 spawnPos = shooter.position + (shooter.rotation * spawnOffset);

            // 5. Spawn đạn từ Pool
            GameObject projGO = ObjectPoolManager.SpawnObject(
                projectilePrefab,
                spawnPos,
                shooter.rotation, // Bắn theo hướng của tháp
                ObjectPoolManager.PoolType.TowerProjectile
            );

            // 6. Khởi chạy đạn (cho đạn "thông minh")
            IProjectile projectile = projGO.GetComponent<IProjectile>();
            if (projectile != null)
            {
                // Chỉ truyền mục tiêu hiện tại cho đạn
                projectile.Launch(new List<GameObject> { currentTarget }, damage);
            }

            // 7. Gán vận tốc (cho đạn "ngu")
            Rigidbody rb = projGO.GetComponent<Rigidbody>();
            if (rb != null)
      
            {
                // (Truy cập biến 'projectileSpeed' từ lớp cha)
                rb.linearVelocity = shootDir * base.projectileSpeed;
            }

            // 8. Tự hủy đạn (nếu bạn không dùng Coroutine trong CannonShot)
            // runner.StartCoroutine(ReturnProjectileToPool(projGO, base.projectileLifeTime));
            // (Lưu ý: Nếu CannonShot đã có logic Coroutine tự hủy, bạn không cần dòng này)

            // 9. Chờ (nếu chưa phải viên cuối)
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(timeBetweenShots);
            }
        }
    }

    // (Bạn có thể thêm hàm Coroutine tự hủy này nếu lớp CannonShot chưa có)
    // private IEnumerator ReturnProjectileToPool(GameObject projectile, float delay)
    // {
    //     yield return new WaitForSeconds(delay);
    //     if (projectile != null && projectile.activeInHierarchy)
    //     {
    //         ObjectPoolManager.ReturnObject(projectile);
    //     }
    // }
}