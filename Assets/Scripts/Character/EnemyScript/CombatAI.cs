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
       
        GameObject idealTarget = FindBestTarget();
        currentTarget = idealTarget; // Ghi đè mục tiêu cũ (nếu có)

        // --- 2. NẾU KHÔNG CÓ MỤC TIÊU -> ĐI ĐƯỜNG ---
        if (currentTarget == null)
        {
            // Nếu vừa mất mục tiêu, quay về path
            if (currentState != AIState.MovingOnPath)
            {
                mover.UpdatePathToClosestNode();
                currentState = AIState.MovingOnPath;
            }
            return; // Không có mục tiêu, không làm gì nữa
        }

        // --- 3. NẾU CÓ MỤC TIÊU -> QUYẾT ĐỊNH TRẠNG THÁI ---

        // Lấy vị trí phẳng của AI
        Vector3 myFlatPos = transform.position;
        myFlatPos.y = 0;

        // Lấy vị trí phẳng của mục tiêu
        Vector3 currentTargetFlatPos = currentTarget.transform.position;
        currentTargetFlatPos.y = 0;

        // Tính khoảng cách bình phương
        float currentFlatSqrDist = (myFlatPos - currentTargetFlatPos).sqrMagnitude;

        // --- Logic Golem đã được làm rõ ---
        float effectiveAttackRangeSqr = attackRangeSqr; // Mặc định là tầm đánh thường

        if (transform.gameObject.name.Equals("Golem(Clone)"))
        {
            // Golem có tầm đánh = tầm thường * 1.5
            float golemRange = stats.AttackRange * 1.5f;
            effectiveAttackRangeSqr = golemRange * golemRange; // (tương đương attackRangeSqr * 2.25)
        }
        // --- Kết thúc logic Golem ---

        // So sánh với tầm đánh hiệu dụng
        if (currentFlatSqrDist <= effectiveAttackRangeSqr)
        {
            currentState = AIState.Attacking;
        }
        else
        {
            // Mục tiêu vẫn còn, nhưng ngoài tầm đánh -> Đuổi theo
            currentState = AIState.ChasingTarget;
        }
    }

    /// <summary>
    /// (TỐI ƯU & ĐÃ SỬA) Tìm mục tiêu tốt nhất dùng sqrMagnitude
    /// </summary>
    private GameObject FindBestTarget()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, stats.TriggerRange, targetBuffer, targetLayer);
        if (hitCount == 0) return null;

        Vector3 myFlatPos = transform.position;
        myFlatPos.y = 0;

        GameObject bestUnit = null;
        float minUnitDist = float.MaxValue;
        GameObject bestTower = null;
        float minTowerDist = float.MaxValue;
        GameObject bestBase = null;
        float minBaseDist = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            var col = targetBuffer[i];
            var health = col.GetComponentInParent<IHealthSystem>();

            if (health == null || health.HasDie()) continue;

            // Lấy GameObject cha (nơi có script health)
            GameObject targetObject = (health as Component).gameObject;

            // --- SỬA LỖI TÍNH TOÁN ---
            // Luôn so sánh khoảng cách 2D (đã làm phẳng)
            Vector3 targetFlatPos = targetObject.transform.position;
            targetFlatPos.y = 0;

            // SỬ DỤNG (A - B).sqrMagnitude
            float sqrDist = (myFlatPos - targetFlatPos).sqrMagnitude;

            if (targetObject.CompareTag("Unit"))
            {
                if (sqrDist < minUnitDist) { minUnitDist = sqrDist; bestUnit = targetObject; }
            }
            else if (targetObject.CompareTag("Tower"))
            {
                if (sqrDist < minTowerDist) { minTowerDist = sqrDist; bestTower = targetObject; }
            }
            else if (targetObject.CompareTag("Castle"))
            {
                if (sqrDist < minBaseDist) { minBaseDist = sqrDist; bestBase = targetObject; }
            }
        }

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