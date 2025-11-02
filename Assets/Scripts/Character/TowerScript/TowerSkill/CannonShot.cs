using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cannon Shot", menuName = "Tower Skill/CannonShot")]
public class CannonShot : ATowerSkill
{
    [Header("Projectile Settings")]
    [SerializeField] internal float projectileSpeed = 20f;
    [SerializeField] internal float aimHeightOffset = 0.5f; // Ngắm cao hơn chân
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0, 1f); // Vị trí nòng súng
    [SerializeField] private float projectileLifeTime = 6f;

    [Header("Burst Settings")]
    [Tooltip("Bắn bao nhiêu viên đạn 1 lần? (1 = bắn đơn)")]
    [SerializeField] private int burstCount = 1;
    [Tooltip("Thời gian chờ giữa các viên đạn (nếu burstCount > 1)")]
    [SerializeField] private float timeBetweenShots = 0.2f;

    /// <summary>
    /// Hàm DoAttack chính, chỉ chịu trách nhiệm khởi động Coroutine
    /// </summary>
    public override void DoAttack(MonoBehaviour runner, Transform shooter, GameObject projectilePrefab, List<GameObject> targets, float damage)
    {
        // 1. Kiểm tra cơ bản
        if (targets == null || projectilePrefab == null) return;

        // 2. Tìm TowerController (để chạy Coroutine)
        TowerController towerController = runner.GetComponent<TowerController>();
        if (towerController == null)
        {
            towerController = runner.GetComponentInParent<TowerController>();
            if (towerController == null)
            {
                Debug.LogWarning("CannonShot: Không tìm thấy TowerController để chạy Coroutine!");
                return;
            }
        }

        // 3. Khởi động Coroutine bắn (sẽ chạy 1 lần hoặc nhiều lần)
        towerController.StartCoroutine(FireSequence(runner, shooter, projectilePrefab, targets, damage));
    }

    /// <summary>
    /// Coroutine xử lý bắn 1 hoặc nhiều viên
    /// </summary>
    private IEnumerator FireSequence(MonoBehaviour runner, Transform shooter, GameObject projectilePrefab, List<GameObject> targets, float damage)
    {
        Transform towerBase = runner.transform; // 'runner' là bệ tháp

        // Vòng lặp này sẽ chạy 'burstCount' lần
        // Nếu burstCount = 1, nó chạy 1 lần (bắn đơn)
        // Nếu burstCount = 3, nó chạy 3 lần (bắn liên thanh)
        for (int i = 0; i < burstCount; i++)
        {
            // 1. Kiểm tra và lấy mục tiêu (phải kiểm tra lại mỗi lần bắn)
            GameObject currentTarget = targets.FirstOrDefault(t => t != null && t.activeInHierarchy);
            if (currentTarget == null)
            {
                yield break; // Hết mục tiêu, dừng burst
            }

            // 2. Quay Bệ (Y-axis)
            Vector3 flatDir = currentTarget.transform.position - towerBase.position;
            flatDir.y = 0;
            if (flatDir.sqrMagnitude > 0.001f)
                towerBase.rotation = Quaternion.LookRotation(flatDir.normalized, Vector3.up);

            // 3. Quay Nòng (X-axis)
            Vector3 aimTargetPos = currentTarget.transform.position + Vector3.up * aimHeightOffset;
            Vector3 shootDir = (aimTargetPos - shooter.position).normalized; // 'shooter' là nòng súng

            shooter.rotation = Quaternion.LookRotation(shootDir);

            // 4. Tính toán vị trí spawn
            Vector3 spawnPos = shooter.position + (shooter.rotation * spawnOffset);

            // 5. Spawn đạn
            GameObject projGO = ObjectPoolManager.SpawnObject(
                projectilePrefab,
                spawnPos,
                shooter.rotation,
                ObjectPoolManager.PoolType.TowerProjectile
            );

            // 6. Launch (cho đạn "thông minh")
            IProjectile projectile = projGO.GetComponent<IProjectile>();
            if (projectile != null)
            {
                Stats shooterStats = runner.GetComponentInParent<Stats>();
                if (shooterStats != null)
                {
                    projectile.Launch(new List<GameObject> { currentTarget }, shooterStats.AttackDamage);
                }
            }

            // 7. Gán vận tốc (cho đạn "ngu")
            Rigidbody rb = projGO.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = shootDir * projectileSpeed;
            }

            // 8. Đặt thời gian tự hủy (trả về pool)
            runner.StartCoroutine(ReturnProjectileToPool(projGO, projectileLifeTime));

            // 9. Chờ (nếu đây chưa phải viên cuối)
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(timeBetweenShots);
            }
        }
    }

    /// <summary>
    /// Hàm hỗ trợ: Tự động trả đạn về Pool sau một thời gian
    /// </summary>
    private IEnumerator ReturnProjectileToPool(GameObject projectile, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (projectile != null && projectile.activeInHierarchy)
        {
            ObjectPoolManager.ReturnObject(projectile);
        }
    }
}