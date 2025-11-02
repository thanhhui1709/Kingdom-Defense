using System.Collections; // Cần cho Coroutine
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Skill", menuName = "Tower Skill/BallistaShot")]
public class BallistaShot : ATowerSkill
{
    [Header("Targeting")]
    [SerializeField] private int numberOfEnemy; // Số mục tiêu tối đa

    [Header("Aiming & Spawn")]
    [Tooltip("Vị trí lệch so với 'shooter' (nòng súng)")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0, 1f);
    [Tooltip("Độ cao ngắm (so với chân địch)")]
    [SerializeField] private float aimHeightOffset = 0.5f;
    [SerializeField] Quaternion rotationFix = Quaternion.Euler(90f, 0f, 0f); // Sửa lỗi model quay sai
    // Tối ưu: Tạo list tạm 1 lần để tránh rác (garbage)
    private List<(GameObject target, float sqrDist)> tempTargetList = new List<(GameObject, float)>();
    private Animator animator;

    /// <summary>
    /// Hàm DoAttack chính (đã được dọn dẹp)
    /// </summary>
    public override void DoAttack(MonoBehaviour runner, Transform shooter, GameObject projectile, List<GameObject> targets, float damage)
    {
        animator=runner.transform.GetComponent<Animator>();

        List<(GameObject target, float sqrDist)> validTargets = GetValidSortedTargets(shooter.position, targets);

      
        if (validTargets == null || validTargets.Count == 0)
            return;

   
        GameObject primaryTarget = validTargets[0].target;
        AimTower(runner.transform, shooter, primaryTarget);

        // --- 3. BẮN ĐẠN ---
     
        Vector3 spawnPosition = shooter.position + (shooter.rotation * spawnOffset);
     

        // Quyết định số lượng đạn sẽ bắn
        int targetsToShoot = Mathf.Min(numberOfEnemy, validTargets.Count);

        // Gọi hàm spawn
        SpawnProjectiles(validTargets, targetsToShoot, projectile, spawnPosition, Quaternion.identity, damage,animator);
    }

    /// <summary>
    /// (HÀM MỚI) Lọc, kiểm tra và sắp xếp các mục tiêu
    /// </summary>
    private List<(GameObject target, float sqrDist)> GetValidSortedTargets(Vector3 shooterPos, List<GameObject> allTargets)
    {
        tempTargetList.Clear(); 

        foreach (var t in allTargets)
        {
            // Kiểm tra cơ bản
            if (t == null || !t.activeInHierarchy) continue;

            // Kiểm tra máu 
            IHealthSystem health = t.GetComponentInParent<IHealthSystem>();
            if (health != null && health.HasDie()) continue;

            // Thêm vào danh sách tạm
            float sqrDist = (t.transform.position - shooterPos).sqrMagnitude;
            tempTargetList.Add((t, sqrDist));
        }

        if (tempTargetList.Count == 0)
            return null;

        // Sắp xếp
        tempTargetList.Sort((a, b) => a.sqrDist.CompareTo(b.sqrDist));
        return tempTargetList;
    }

    /// <summary>
    /// (HÀM MỚI) Xử lý logic xoay bệ tháp và nòng súng
    /// </summary>
    private void AimTower(Transform towerBase, Transform barrel, GameObject primaryTarget)
    {
       
        Vector3 aimTargetPos = primaryTarget.transform.position + Vector3.up * aimHeightOffset;
        Vector3 shootDir = (aimTargetPos - barrel.position).normalized;
        barrel.rotation = Quaternion.LookRotation(-shootDir);
    }


    private void SpawnProjectiles(List<(GameObject target, float sqrDist)> sortedTargets, int count, GameObject projectilePrefab, Vector3 spawnPos, Quaternion spawnRot_Ignored, float damage,Animator anim)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject target = sortedTargets[i].target;

          
            Vector3 aimTargetPos = target.transform.position + Vector3.up * aimHeightOffset;

         
            Vector3 shootDir = (aimTargetPos - spawnPos).normalized;

            Quaternion spawnRotation = Quaternion.LookRotation(shootDir);

            // --- KẾT THÚC TÍNH TOÁN ---

            GameObject go = ObjectPoolManager.SpawnObject(
                projectilePrefab,
                spawnPos,
                spawnRotation* rotationFix, // Dùng hướng xoay MỚI (chính xác 100%)
                ObjectPoolManager.PoolType.TowerProjectile
            );
            anim.SetTrigger("trig_Attack");

            IProjectile projectileComp = go.GetComponent<IProjectile>();

            if (projectileComp == null)
            {
                Debug.LogError("Projectile does not implement IProjectile interface.");
                ObjectPoolManager.ReturnObject(go);
                continue;
            }

            // Giao mục tiêu cho đạn
            projectileComp.Launch(new List<GameObject> { target }, damage);
        }
    }
}