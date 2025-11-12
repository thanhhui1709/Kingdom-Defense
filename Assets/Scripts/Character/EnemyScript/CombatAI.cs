using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Stats), typeof(MovementController))]
public class CombatAI : MonoBehaviour
{
    private enum AIState
    {
        MovingOnPath,
        ChasingTarget,
        Attacking
    }

    [SerializeField] private LayerMask targetLayer;
    private Stats stats;
    private MovementController mover;
    private AnimationController anim;

    private GameObject currentTarget;
    private AIState currentState;

    // --- BIẾN MỚI ĐỂ SỬA LỖI ANIMATION ---
    private float attackCooldownTimer = 0f;

    // Tối ưu hóa: Bộ đệm (buffer) để tìm mục tiêu
    private Collider[] targetBuffer = new Collider[50];
    private Collider currentTargetCollider; // <-- THÊM MỚI
    private float triggerRangeSqr;
    private float attackRangeSqr;
    private EnemyHealth enemyHealth;
    void Awake()
    {
        stats = GetComponent<Stats>();
        mover = GetComponent<MovementController>();
        anim = GetComponent<AnimationController>();
        enemyHealth = GetComponent<EnemyHealth>();

        // Tính bình phương tầm bắn 1 LẦN
        triggerRangeSqr = stats.TriggerRange * stats.TriggerRange;
        attackRangeSqr = stats.AttackRange * stats.AttackRange;
    }

    /// <summary>
    /// Hàm MỘT cửa: Cập nhật mục tiêu và quyết định trạng thái
    /// </summary>
    private void Update()
    {
        if (enemyHealth.HasDie()) return;
        // Cập nhật cooldown tấn công
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
        UpdateStateAndTarget();
        ExecuteAction();
    }
    private void UpdateStateAndTarget()
    {
        // --- 1. TÌM MỤC TIÊU (GIỜ TRẢ VỀ COLLIDER) ---
        currentTargetCollider = FindBestTarget(); // Gán collider mới

        // --- 2. NẾU KHÔNG CÓ MỤC TIÊU -> ĐI ĐƯỜNG ---
        if (currentTargetCollider == null)
        {
            currentTarget = null; // Đảm bảo clear target

            if (currentState != AIState.MovingOnPath)
            {
                mover.ResumePathMovement();
                currentState = AIState.MovingOnPath;
            }
            return; // Không có mục tiêu, không làm gì nữa
        }

        // Lấy GameObject cha từ collider (nơi có script Health)
        IHealthSystem health = currentTargetCollider.GetComponentInParent<IHealthSystem>();

        // Lấy GameObject bằng cách ép kiểu về Component
        currentTarget = (health as Component).gameObject;

        // --- 3. NẾU CÓ MỤC TIÊU -> TÍNH TOÁN KHOẢNG CÁCH CHÍNH XÁC ---

        // Lấy tầm đánh hiệu dụng (với logic Golem)
        float effectiveAttackRange = stats.AttackRange;
        if (transform.gameObject.name.Equals("Golem(Clone)"))
        {
            effectiveAttackRange *= 1.5f;
        }
        float attackRangeSqr = effectiveAttackRange * effectiveAttackRange;

        // --- SỬA LỖI LOGIC TÍNH KHOẢNG CÁCH ---
        // Thay vì tính (Tâm-đến-Tâm),
        // chúng ta tính (Tâm-của-TA đến MÉP-gần-nhất-của-ĐỊCH)

        // 1. Tìm điểm gần nhất trên BỀ MẶT collider của địch
        Vector3 closestPointOnTarget = currentTargetCollider.ClosestPoint(transform.position);

        // 2. Tính khoảng cách bình phương từ TA (tâm) đến điểm đó
        // (Không cần làm phẳng Y, vì ClosestPoint đã xử lý 3D)
        float currentSqrDist = (transform.position - closestPointOnTarget).sqrMagnitude;
        // --- KẾT THÚC SỬA ---

        // So sánh
        if (currentSqrDist <= attackRangeSqr)
        {
            currentState = AIState.Attacking;
        }
        else
        {
            currentState = AIState.ChasingTarget;
        }
    }

    /// <summary>
    /// (SỬA LẠI) Tìm mục tiêu tốt nhất, TRẢ VỀ COLLIDER
    /// </summary>
    private Collider FindBestTarget() // <-- ĐỔI KIỂU TRẢ VỀ
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, stats.TriggerRange, targetBuffer, targetLayer);
        if (hitCount == 0) return null;

        Vector3 myFlatPos = transform.position;
        myFlatPos.y = 0;

        // --- Đổi kiểu biến ---
        Collider bestUnit = null;
        float minUnitDist = float.MaxValue;
        Collider bestTower = null;
        float minTowerDist = float.MaxValue;
        Collider bestBase = null;
        float minBaseDist = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            var col = targetBuffer[i]; // 'col' chính là collider
            var health = col.GetComponentInParent<IHealthSystem>();

            if (health == null || health.HasDie()) continue;

            GameObject targetObject = (health as Component).gameObject;

            // (Tính toán sqrDist vẫn giữ nguyên)
            Vector3 targetFlatPos = targetObject.transform.position;
            targetFlatPos.y = 0;
            float sqrDist = (myFlatPos - targetFlatPos).sqrMagnitude;

            // --- Gán 'col' thay vì 'targetObject' ---
            if (targetObject.CompareTag("Unit"))
            {
                if (sqrDist < minUnitDist) { minUnitDist = sqrDist; bestUnit = col; }
            }
            else if (targetObject.CompareTag("Tower"))
            {
                if (sqrDist < minTowerDist) { minTowerDist = sqrDist; bestTower = col; }
            }
            else if (targetObject.CompareTag("Castle"))
            {
                if (sqrDist < minBaseDist) { minBaseDist = sqrDist; bestBase = col; }
            }
        }

        // Trả về collider ưu tiên
        if (bestUnit != null) return bestUnit;
        if (bestTower != null) return bestTower;
        if (bestBase != null) return bestBase;
        return null;
    }

    /// <summary>
    /// Thực thi hành động tương ứng với trạng thái.
    /// </summary>
    private void ExecuteAction()
    {
        switch (currentState)
        {
            case AIState.MovingOnPath:
                mover.ResumePathMovement();
                anim.PlaySpecialAnimation(AnimationType.Walk); // Đảm bảo chạy anim Walk
                break;

            case AIState.ChasingTarget:
                mover.MoveTowards(currentTarget.transform.position);
                anim.PlaySpecialAnimation(AnimationType.Walk); // Đảm bảo chạy anim Walk
                break;

            case AIState.Attacking:
                mover.StopMovement(); // Dừng di chuyển

  
                if (attackCooldownTimer <= 0f)
                {
                    AttackTarget(); // Gọi hàm tấn công (chỉ 1 lần)
                    attackCooldownTimer = 1f / stats.AttackSpeed; // Reset cooldown
                }
               
                break;
        }
    }

    /// <summary>
    /// (SỬA LẠI) Hàm này giờ chỉ lo gọi animation
    /// </summary>
    private void AttackTarget()
    {
        transform.LookAt(currentTarget.transform);
        anim.PlaySpecialAnimation(AnimationType.Attack);
        Debug.Log("Attacking " + currentTarget.name);
    }

    // (Hàm AnimationEvent_HitTarget giữ nguyên)
    public void AnimationEvent_HitTarget(int id)
    {
        if (currentTarget == null) return;
        AttackBehavior attackToUse = stats.GetAttack(id);
        attackToUse=Instantiate(attackToUse); // Phải Instantiate để tránh xung đột khi nhiều kẻ tấn công cùng lúc
        if (attackToUse != null)
        {
            attackToUse.Execute(this, stats, currentTarget);
        }
    }
}