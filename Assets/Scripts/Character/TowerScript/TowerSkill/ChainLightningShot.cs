using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Skill", menuName = "Tower Skill/ChainLightningShot")]
public class ChainLightningShot : ATowerSkill
{
    [Header("Chain Lightning Stats")]
    [SerializeField] private int numberOfChains = 3;
    [SerializeField] private float chainRange = 8f;

    [Header("Targeting")]
    [Tooltip("Đặt Layer của kẻ địch vào đây để tối ưu hóa việc tìm kiếm.")]
    [SerializeField] private LayerMask enemyLayer; // Biến mới để chỉ định Layer của địch

    public override void DoAttack(Transform shooter, GameObject projectile, List<GameObject> targetsInRange, float damage)
    {
        if (targetsInRange.Count == 0) return;

        // --- Giai đoạn 1: Vẫn tìm mục tiêu chính trong tầm bắn của trụ ---
        GameObject primaryTarget = targetsInRange
            .OrderBy(e => Vector3.Distance(shooter.position, e.transform.position))
            .FirstOrDefault();

        if (primaryTarget == null) return;

        List<GameObject> hitEnemies = new List<GameObject>();
        hitEnemies.Add(primaryTarget);

        CreateLightningBolt(projectile, shooter.position, primaryTarget.transform.position);
        // Gây sát thương cho mục tiêu chính

        GameObject currentTarget = primaryTarget;

        // --- Giai đoạn 2: Vòng lặp tìm mục tiêu lan truyền từ vị trí của mục tiêu trước đó ---
        for (int i = 0; i < numberOfChains; i++)
        {
            // === THAY ĐỔI LỚN BẮT ĐẦU TỪ ĐÂY ===
            // Thực hiện một cuộc tìm kiếm vật lý MỚI xung quanh mục tiêu vừa bị đánh
            Collider[] nearbyColliders = Physics.OverlapSphere(currentTarget.transform.position, chainRange, enemyLayer);

            // Tìm kẻ địch gần nhất trong số những kẻ địch vừa tìm thấy và chưa bị đánh
            GameObject nextTarget = FindClosestEnemyInColliders(currentTarget.transform.position, nearbyColliders, hitEnemies);

            if (nextTarget != null)
            {
                CreateLightningBolt(projectile, currentTarget.transform.position, nextTarget.transform.position);
                // Gây sát thương cho mục tiêu tiếp theo

                hitEnemies.Add(nextTarget);
                currentTarget = nextTarget;
            }
            else
            {
                // Không tìm thấy ai khác trong tầm lan truyền, dừng lại
                break;
            }
        }
    }

    // Hàm phụ để tạo và vẽ tia sét (giữ nguyên)
    private void CreateLightningBolt(GameObject lightningPrefab, Vector3 startPos, Vector3 endPos)
    {
        if(lightningPrefab == null)
        {
            Debug.LogError("lightningPrefab is not assigned.");
            return;
        }
        GameObject boltGO = Instantiate(lightningPrefab, startPos, Quaternion.identity);
        LineRenderer lr = boltGO.GetComponent<LineRenderer>(); 
        if(lr == null)
        {
            Debug.LogError("LineRenderer component not found on the lightningPrefab.");
            Destroy(boltGO);
            return;
        }
        lr.positionCount = 2;
        lr.SetPosition(0, startPos);
        lr.SetPosition(1, endPos);
    }

    // Hàm phụ MỚI để tìm kẻ địch từ kết quả của OverlapSphere
    private GameObject FindClosestEnemyInColliders(Vector3 position, Collider[] colliders, List<GameObject> alreadyHit)
    {
        return colliders
            .Select(c => c.gameObject) // Chuyển từ Collider[] sang GameObject[]
            .Where(go => !alreadyHit.Contains(go)) // Lọc ra những kẻ đã bị đánh
            .OrderBy(go => Vector3.Distance(position, go.transform.position)) // Sắp xếp theo khoảng cách
            .FirstOrDefault(); // Lấy kẻ gần nhất
    }
}