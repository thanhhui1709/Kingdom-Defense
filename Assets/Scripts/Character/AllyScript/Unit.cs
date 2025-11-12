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
    [Tooltip("Lính sẽ tản ra trong bán kính này khi di chuyển")]
    [SerializeField] private float formationSpread = 1.5f; // Logic chống chồng chéo

    // State Management
    private UnitState currentState;
    private GameObject currentTarget;
    private Collider currentTargetCollider; // <-- Dùng để sửa lỗi "tâm sâu"

    // Cooldown timers
    private float searchCooldown = 0.25f; // Chỉ tìm địch/cập nhật path 4 lần/giây
    private float searchTimer = 0f;
    private float attackCooldownTimer = 0f;

    // Logic chống kẹt
    private float stuckTimer = 0f;
    private const float MAX_STUCK_TIME = 2.0f; // Giây

    void Awake()
    {
        mover = GetComponent<UnitMovement>();
        stats = GetComponent<Stats>();
        pathFinder = new PathFinding();
        health = GetComponent<UnitHealth>();
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
        if (searchTimer > 0) searchTimer -= Time.deltaTime;
        if (attackCooldownTimer > 0) attackCooldownTimer -= Time.deltaTime;

        // Chạy State Machine
        switch (currentState)
        {
            case UnitState.Idle:
                HandleIdleState();
                break;
            case UnitState.Moving:
                HandleMovingState(); // <-- SỬA
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
                // Tự động tấn công
                ReceiveAttackCommand(nearbyEnemy);
            }
            searchTimer = searchCooldown;
        }
    }

    // --- SỬA LẠI HÀM NÀY (Logic chống kẹt) ---
    private void HandleMovingState()
    {
        if (!mover.IsMoving()) // 1. Đã đến nơi
        {
            currentState = UnitState.Idle;
            stuckTimer = 0f; // Reset
        }
        else // 2. Vẫn đang trên đường
        {
            anim.Play(AnimationType.Walk); // Đảm bảo đi

            // 3. Kiểm tra xem có bị kẹt không
            if (mover.CurrentSpeed < 0.1f)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > MAX_STUCK_TIME)
                {
                    // Bị kẹt -> Hủy lệnh và tự tìm địch
                    currentState = UnitState.Idle;
                    mover.StopMovement();
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }
        }
    }

    // --- SỬA LẠI HÀM NÀY (Logic "tâm sâu" và cập nhật path) ---
    private void HandleAttackingState()
    {
        // 1. Kiểm tra mục tiêu có hợp lệ không
        if (IsTargetInvalid())
        {
            currentTarget = null;
            currentTargetCollider = null;
            currentState = UnitState.Idle;
            mover.StopMovement();
            return;
        }

        // 2. Kiểm tra khoảng cách (SỬA LỖI "TÂM SÂU")
        // Tính từ mép collider địch, chứ không phải tâm
        Vector3 closestPointOnTarget = currentTargetCollider.ClosestPoint(transform.position);
        float sqrDistance = (transform.position - closestPointOnTarget).sqrMagnitude;
        float attackRangeSqr = stats.AttackRange * stats.AttackRange;

        // 3. Hành động
        if (sqrDistance <= attackRangeSqr)
        {
            // --- TRONG TẦM ---
            mover.StopMovement(); // Dừng pathfinding
            stuckTimer = 0f; // Reset
            transform.LookAt(currentTarget.transform);

            if (attackCooldownTimer <= 0)
            {
                anim.PlaySpecialAnimation(AnimationType.Attack);
                attackCooldownTimer = 1f / stats.AttackSpeed;
            }
        }
        else
        {
            // --- NGOÀI TẦM (Đuổi theo) ---
            anim.Play(AnimationType.Walk, stats.MoveSpeed); // Chơi anim đi

            // Kiểm tra bị kẹt (giống hệt HandleMovingState)
            if (mover.CurrentSpeed < 0.1f)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > MAX_STUCK_TIME)
                {
                    currentState = UnitState.Idle;
                    mover.StopMovement();
                    stuckTimer = 0f;
                    return;
                }
            }
            else
            {
                stuckTimer = 0f;
            }

            // Cập nhật lại đường đi 4 lần/giây (nếu địch di chuyển)
            if (searchTimer <= 0)
            {
                IssuePathfindCommand(currentTarget.transform.position, false);
                searchTimer = searchCooldown; // Reset timer
            }
        }
    }

    /// <summary>
    /// Hàm kiểm tra mục tiêu có còn "tươi" không
    /// </summary>
    private bool IsTargetInvalid()
    {
        if (currentTarget == null || !currentTarget.activeInHierarchy)
            return true;

        if (currentTargetCollider == null) // (Lấy collider nếu chưa có)
            currentTargetCollider = currentTarget.GetComponentInChildren<Collider>();

        IHealthSystem targetHealth = currentTarget.GetComponentInParent<IHealthSystem>();
        if (targetHealth != null && targetHealth.HasDie())
            return true;

        return false;
    }

    // --- HÀM TÌM ĐỊCH (SỬA LẠI) ---
    private GameObject FindClosestEnemyInRange()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, stats.TriggerRange, enemyLayer);
        GameObject closestEnemy = null;
        float minSqrDistance = float.MaxValue;

        foreach (var col in enemies)
        {
            if (col.transform == this.transform) continue;
            IHealthSystem health = col.GetComponentInParent<IHealthSystem>();

            if (health != null && !health.HasDie())
            {
                float sqrDist = (col.transform.position - transform.position).sqrMagnitude;
                if (sqrDist < minSqrDistance)
                {
                    minSqrDistance = sqrDist;
                    closestEnemy = (health as Component).gameObject;
                }
            }
        }
        return closestEnemy;
    }

    // --- CÁC HÀM NHẬN LỆNH TỪ UNIT MANAGER ---

    public void Select() { if (health != null) health.ShowHealthBar(); }
    public void Deselect() { if (health != null) health.HideHealthBar(); }

    // --- SỬA LẠI HÀM NÀY (Logic chống chồng chéo) ---
    public void ReceiveMoveCommand(Vector3 destination)
    {
        currentState = UnitState.Moving;
        currentTarget = null;
        currentTargetCollider = null;
        stuckTimer = 0f;

        // 1. Tạo vị trí lệch ngẫu nhiên
        Vector3 randomOffset = Random.insideUnitSphere * formationSpread;
        randomOffset.y = 0;
        Vector3 finalDestination = destination + randomOffset;

        // 2. Ra lệnh tìm đường
        IssuePathfindCommand(finalDestination, true);
    }

    // --- SỬA LẠI HÀM NÀY (Logic pathfind khi tấn công) ---
    public void ReceiveAttackCommand(GameObject target)
    {
        currentState = UnitState.Attacking;
        currentTarget = target;
        currentTargetCollider = target.GetComponentInChildren<Collider>(); // Lấy collider ngay
        stuckTimer = 0f;

        // Ra lệnh tìm đường (không có offset)
        IssuePathfindCommand(target.transform.position, true);
    }

    /// <summary>
    /// Hàm nội bộ: Tìm và gán đường đi
    /// </summary>
    private void IssuePathfindCommand(Vector3 destination, bool isNewCommand)
    {
        // Nếu là lệnh mới (click) thì hủy di chuyển cũ
        if (isNewCommand) mover.StopMovement();

        PathNode startNode = PathNodeManager.Instance.FindClosestNode(transform.position);
        PathNode endNode = PathNodeManager.Instance.FindClosestNode(destination);

        List<PathNode> newPath = null;
        if (startNode != null && endNode != null)
        {
            newPath = pathFinder.FindPath(startNode, endNode);
        }

        // Gửi path cho "chân"
        mover.SetPath(newPath, destination);
    }

    // (Hàm AttackEnemy giữ nguyên)
    public void AttackEnemy(int animationIndex)
    {
        if (IsTargetInvalid()) return; // Kiểm tra lần cuối

        AttackBehavior attackBehavior = stats.GetAttack(animationIndex);
        attackBehavior = Instantiate(attackBehavior);
        attackBehavior.Execute(this, stats, currentTarget);
    }
}