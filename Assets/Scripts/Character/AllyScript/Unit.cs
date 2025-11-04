using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(UnitMovement), typeof(Stats), typeof(Collider))]
public class Unit : MonoBehaviour
{
    private enum UnitState
    {
        Idle,       // Đứng yên (VÀ tự động tìm địch)
        Moving,     // Đang di chuyển theo lệnh
        Attacking   // Đang đuổi theo/tấn công mục tiêu
    }

    // Components
    private UnitMovement mover;
    private Stats stats;
    private PathFinding pathFinder;
    private UnitHealth health;
    private AnimationController anim;

    [Header("AI Behavior")]
    [SerializeField] private LayerMask enemyLayer;

    // State Management
    private UnitState currentState;
    private GameObject currentTarget;

    // Cooldown timers
    private float searchCooldown = 0.25f; // Chỉ tìm địch 4 lần/giây
    private float searchTimer = 0f;

    // --- THÊM MỚI: SỬA LỖI SPAM ANIMATION ---
    private float attackCooldownTimer = 0f;

    void Awake()
    {
        mover = GetComponent<UnitMovement>();
        stats = GetComponent<Stats>();
        pathFinder = new PathFinding();
        health = GetComponent<UnitHealth>(); // Giả sử UnitHealth tồn tại
        anim = GetComponent<AnimationController>();
    }

    void OnEnable()
    {
        UnitController.Instance.RegisterUnit(this);
        currentState = UnitState.Idle;
    }

    void OnDisable()
    {
        if (UnitController.Instance != null)
        {
            UnitController.Instance.UnregisterUnit(this);
        }
    }

    void Update()
    {
        // Luôn đếm ngược các timer
        if (searchTimer > 0)
        {
            searchTimer -= Time.deltaTime;
        }
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        // Chạy State Machine
        switch (currentState)
        {
            case UnitState.Idle:
                HandleIdleState();
                break;

            case UnitState.Moving:
                if (!mover.IsMoving())
                {
                    currentState = UnitState.Idle;
                   
                }
                else
                {
                    anim.Play(AnimationType.Walk); // Đảm bảo đi
                }
                break;

            case UnitState.Attacking:
                HandleAttackingState();
                break;
        }
    }

    // --- LOGIC CỦA TỪNG TRẠNG THÁI ---

    private void HandleIdleState()
    {
     
        if (searchTimer <= 0)
        {
            GameObject nearbyEnemy = FindClosestEnemyInRange();
            if (nearbyEnemy != null)
            {
                currentTarget = nearbyEnemy;
                currentState = UnitState.Attacking;
            }
            searchTimer = searchCooldown;
        }
    }

    private void HandleAttackingState()
    {
        // --- SỬA LỖI CHÍNH: KIỂM TRA MỤC TIÊU ---

        // 1. Kiểm tra mục tiêu có hợp lệ không
        bool isTargetInvalid = false;
        if (currentTarget == null || !currentTarget.activeInHierarchy)
        {
            isTargetInvalid = true;
        }
        else
        {
            // Lấy IHealthSystem (an toàn, dùng GetComponentInParent)
            IHealthSystem targetHealth = currentTarget.GetComponentInParent<IHealthSystem>();

            // Nếu mục tiêu đã chết -> coi như không hợp lệ
            if (targetHealth != null && targetHealth.HasDie())
            {
                isTargetInvalid = true;
            }
        }

        // 2. Nếu không hợp lệ, quay về Idle
        if (isTargetInvalid)
        {
            currentTarget = null;
            currentState = UnitState.Idle;
            mover.StopMovement();
            return;
        }
        // --- KẾT THÚC SỬA ---

        // 3. Kiểm tra khoảng cách (Mục tiêu HỢP LỆ)
        Vector3 directionVector = currentTarget.transform.position - transform.position;
        directionVector.y = 0;
        float sqrDistance = directionVector.sqrMagnitude;
        float attackRangeSqr = stats.AttackRange * stats.AttackRange;

        // 4. Hành động
        if (sqrDistance <= attackRangeSqr)
        {
            // Trong tầm: Dừng lại và tấn công
            mover.StopMovement();
            transform.LookAt(currentTarget.transform); // Xoay mặt

            // --- SỬA LỖI SPAM ANIMATION ---
            if (attackCooldownTimer <= 0)
            {
                anim.PlaySpecialAnimation(AnimationType.Attack);
                attackCooldownTimer = 1f / stats.AttackSpeed; // Reset cooldown
            }
           
        }
        else
        {
            // Ngoài tầm: Đuổi theo
            mover.MoveTowards(currentTarget.transform.position);
            anim.Play(AnimationType.Walk,stats.MoveSpeed); // Chơi anim đi
        }
    }

    /// <summary>
    /// MỚI: Hàm quét tìm kẻ địch xung quanh
    /// </summary>
    private GameObject FindClosestEnemyInRange()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, stats.TriggerRange, enemyLayer);

        GameObject closestEnemy = null;
        float minSqrDistance = float.MaxValue;

        foreach (var col in enemies)
        {
            if (col.transform == this.transform) continue;

            // --- SỬA LỖI CHÍNH: KIỂM TRA MỤC TIÊU ---
            // Phải lấy IHealthSystem để kiểm tra
            IHealthSystem health = col.GetComponentInParent<IHealthSystem>();

            // Chỉ coi là mục tiêu nếu nó còn sống
            if (health != null && !health.HasDie())
            {
                float sqrDist = (col.transform.position - transform.position).sqrMagnitude;
                if (sqrDist < minSqrDistance)
                {
                    minSqrDistance = sqrDist;
                    // Gán GameObject cha (nơi có script Health) làm mục tiêu
                    closestEnemy = (health as Component).gameObject;
                }
            }
            // --- KẾT THÚC SỬA ---
        }
        return closestEnemy;
    }


    // --- CÁC HÀM NHẬN LỆNH TỪ UNIT MANAGER ---

    public void Select()
    {
        if (health != null) health.ShowHealthBar();
    }

    public void Deselect()
    {
        if (health != null) health.HideHealthBar();
    }

    public void ReceiveMoveCommand(Vector3 destination)
    {
        currentState = UnitState.Moving;
        currentTarget = null; // Hủy lệnh tấn công

        PathNode startNode = PathNodeManager.Instance.FindClosestNode(transform.position);
        PathNode endNode = PathNodeManager.Instance.FindClosestNode(destination);
        if (startNode == null || endNode == null) return;

        List<PathNode> newPath = pathFinder.FindPath(startNode, endNode);
        mover.SetPath(newPath, destination);
    }

    public void ReceiveAttackCommand(GameObject target)
    {
        currentState = UnitState.Attacking;
        currentTarget = target;
    }

    // Hàm này được gọi bởi Animation Event
    public void AttackEnemy(int animationIndex)
    {
        // Kiểm tra mục tiêu lần cuối trước khi gây sát thương
        if (currentTarget == null) return;
        var health = currentTarget.GetComponentInParent<IHealthSystem>();
        if (health != null && health.HasDie()) return;

        // Gây sát thương
        AttackBehavior attackBehavior = stats.GetAttack(animationIndex);
        attackBehavior = Instantiate(attackBehavior);
        attackBehavior.Execute(this, stats, currentTarget);
    }
}