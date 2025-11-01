using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Burst Cannon Shot", menuName = "Tower Skill/BurstCannonShot")]
public class BurstCannonShot : CannonShot
{
    [SerializeField] private int burstCount ;            // Số viên bắn liên tiếp
    [SerializeField] private float timeBetweenShots = 0.2f; // Thời gian giữa các viên

    public override void DoAttack(Transform shooter, GameObject projectilePrefab, List<GameObject> targets, float damage)
    {
      
        TowerController towerController = shooter.GetComponent<TowerController>();
        if (towerController == null)
        {
            towerController = shooter.GetComponentInParent<TowerController>();
            if (towerController == null)
            {
                Debug.LogWarning("BurstCannonShot: Không tìm thấy TowerController để chạy Coroutine!");
                return;
            }
        }

        // Dùng chính TowerController để chạy Coroutine
        towerController.StartCoroutine(BurstFire(shooter, projectilePrefab, targets, damage));
    }

    private IEnumerator BurstFire(Transform shooter, GameObject projectilePrefab, List<GameObject> targets, float damage)
    {
        for (int i = 0; i < burstCount; i++)
        {
            base.DoAttack(shooter, projectilePrefab, targets, damage); // Bắn 1 viên
            yield return new WaitForSeconds(timeBetweenShots);         // Chờ rồi bắn tiếp
        }
    }
}
