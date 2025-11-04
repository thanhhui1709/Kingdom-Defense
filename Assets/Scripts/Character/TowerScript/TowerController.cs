using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Stats))]
public class TowerController : MonoBehaviour
{
    private Stats stats;
    [SerializeField] private ATowerSkill towerSkill;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int maxTarget = 1;
    [SerializeField] private GameObject shooter;
    [SerializeField] private LayerMask enemyLayer; // THÊM MỚI: Chỉ định layer của địch

    private enum TowerState { Idle, Attacking }
    private TowerState currentState = TowerState.Idle;

    // Dùng HashSet để tối ưu việc Thêm/Xóa
    private HashSet<GameObject> currentTarget = new();
    private float attackCooldown;
    private float attackRangeSqr; // Tối ưu: Tính bình phương tầm bắn 1 lần

    // Tối ưu: Bộ đệm (buffer) để tránh tạo rác (garbage)
    private Collider[] colliderBuffer = new Collider[100];
    private List<TargetInfo> potentialTargets = new List<TargetInfo>(100);

    // Struct nội bộ để Sắp xếp (Sort) nhanh
    private struct TargetInfo
    {
        public GameObject target;
        public float sqrDistance;
        public TargetInfo(GameObject t, float d) { target = t; sqrDistance = d; }
    }

    void Start()
    {
        stats = GetComponent<Stats>();
        // Tính trước bình phương tầm bắn để so sánh nhanh hơn
        attackRangeSqr = stats.AttackRange * stats.AttackRange;
    }

    void Update()
    {
        // Luôn chạy máy trạng thái (state machine)
        ExecuteState(Time.deltaTime);
    }

    private void ExecuteState(float deltaTime)
    {
        // 1. Luôn đếm ngược cooldown
        if (attackCooldown > 0)
            attackCooldown -= deltaTime;

        // 2. Luôn dọn dẹp mục tiêu đã chết/ra khỏi tầm
        ValidateTargets();

        switch (currentState)
        {
            case TowerState.Idle:
                // Khi rảnh, tìm mục tiêu mới
                FindNewTargets();

                // Nếu tìm thấy, chuyển trạng thái
                if (currentTarget.Count > 0)
                    currentState = TowerState.Attacking;
                break;

            case TowerState.Attacking:
                // Nếu không còn mục tiêu, quay về rảnh
                if (currentTarget.Count == 0)
                {
                    currentState = TowerState.Idle;
                    return;
                }

                // Nếu còn chỗ trống, tìm thêm mục tiêu
                FindNewTargets();

                // Tấn công nếu đã hết cooldown
                if (attackCooldown <= 0)
                {
                    Attack();
                    attackCooldown = 1f / stats.AttackSpeed;
                }
                break;
        }
    }

    /// <summary>
    /// (SỬA LỖI CHÍNH) Dọn dẹp danh sách mục tiêu
    /// Xóa mục tiêu nếu nó null, không hoạt động, ra khỏi tầm, HOẶC ĐÃ CHẾT
    /// </summary>
    private void ValidateTargets()
    {
        if (currentTarget.Count == 0) return;

        // Dùng RemoveWhere của HashSet (rất hiệu quả)
        currentTarget.RemoveWhere(target =>
            target == null ||
            !target.activeInHierarchy ||
            Vector3.Distance(transform.position, target.transform.position) > stats.AttackRange ||
            target.GetComponent<IHealthSystem>()?.HasDie() == true // <--- SỬA LỖI Ở ĐÂY
        );
    }

    /// <summary>
    /// (TỐI ƯU HÓA) Tìm và thêm mục tiêu mới nếu còn chỗ trống
    /// </summary>
    private void FindNewTargets()
    {
        int targetsNeeded = maxTarget - currentTarget.Count;
        if (targetsNeeded <= 0) return; // Đã đủ mục tiêu

        // Xóa danh sách tạm
        potentialTargets.Clear();

        // 1. Dùng OverlapSphereNonAlloc (nhanh, không tạo rác)
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, stats.AttackRange, colliderBuffer, enemyLayer);

        for (int i = 0; i < hitCount; i++)
        {
            GameObject target = colliderBuffer[i].gameObject;

            // 2. Lọc: Bỏ qua nếu đã có trong danh sách
            if (target != null && !currentTarget.Contains(target))
            {
                // Lọc: Bỏ qua nếu không có IHealthSystem hoặc đã chết
                var health = target.GetComponent<IHealthSystem>();
                if (health != null && !health.HasDie())
                {
                    // 3. Thêm vào danh sách tạm để Sắp xếp
                    float sqrDist = Vector3.Distance(transform.position, target.transform.position);
                    potentialTargets.Add(new TargetInfo(target, sqrDist));
                }
            }
        }

        // 4. Không có mục tiêu mới
        if (potentialTargets.Count == 0) return;

        // 5. Sắp xếp danh sách tạm (chỉ sắp xếp các mục tiêu mới, nhanh hơn LINQ)
        potentialTargets.Sort((a, b) => a.sqrDistance.CompareTo(b.sqrDistance));

        // 6. Thêm N mục tiêu gần nhất vào danh sách chính
        int count = Mathf.Min(targetsNeeded, potentialTargets.Count);
        for (int i = 0; i < count; i++)
        {
            currentTarget.Add(potentialTargets[i].target);
        }
    }

    /// <summary>
    /// Hàm hỗ trợ: thực hiện tấn công
    /// </summary>
    private void Attack()
    {
        // Chuyển HashSet thành List cho hàm DoAttack
        towerSkill.DoAttack(this,shooter.transform, projectilePrefab, currentTarget.ToList(), stats.AttackDamage);
    }

    /// <summary>
    /// (SỬA LỖI) Bật Gizmos để xem tầm bắn (chỉ chạy khi stats đã được load)
    /// </summary>
  
    public HashSet<GameObject> GetCurrentTargets()
    {
        return currentTarget;
    }
}
