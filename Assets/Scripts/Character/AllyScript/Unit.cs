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


    // MỚI: Thêm Layer của địch để quét
    [Header("AI Behavior")]
    [SerializeField] private LayerMask enemyLayer;

    // State Management
    private UnitState currentState;
    private GameObject currentTarget;

    // MỚI: Biến đếm thời gian để tối ưu hóa việc tìm kiếm
    private float searchCooldown = 0.25f; // Chỉ tìm địch 4 lần/giây
    private float searchTimer = 0f;
    private UnitHealth health;
    private AnimationController anim;

    void Awake()
    {
        mover = GetComponent<UnitMovement>();
        stats = GetComponent<Stats>();
        pathFinder = new PathFinding();
        health = GetComponent<UnitHealth>();
        anim = GetComponent<AnimationController>();

     
    }

    void Start()
    {
        UnitManager.Instance.RegisterUnit(this);
        currentState = UnitState.Idle;
    }

    void OnDestroy()
    {
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.UnregisterUnit(this);
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case UnitState.Idle:
                // MỚI: Gọi logic của trạng thái Idle
                HandleIdleState();
                break;

            case UnitState.Moving:
                if (!mover.IsMoving())
                {
                    // Đã đến nơi, chuyển sang Idle (để bắt đầu tìm địch)
                    currentState = UnitState.Idle;
                }
                break;

            case UnitState.Attacking:
                HandleAttackingState();
                break;
        }

        // MỚI: Cập nhật bộ đếm thời gian
        if (searchTimer > 0)
        {
            searchTimer -= Time.deltaTime;
        }
    }

    // --- LOGIC CỦA TỪNG TRẠNG THÁI ---

    /// <summary>
    /// MỚI: Logic khi đứng yên (chờ lệnh HOẶC tự tìm địch)
    /// </summary>
    private void HandleIdleState()
    {
        // Nếu đã đến lúc tìm kiếm
        if (searchTimer <= 0)
        {
            GameObject nearbyEnemy = FindClosestEnemyInRange();
            if (nearbyEnemy != null)
            {
                // Tìm thấy! Chuyển sang trạng thái Tấn công
                currentTarget = nearbyEnemy;
                currentState = UnitState.Attacking;
            }

            // Đặt lại bộ đếm
            searchTimer = searchCooldown;
        }
    }

    /// <summary>
    /// Logic khi đang ở trạng thái Tấn công
    /// </summary>
    private void HandleAttackingState()
    {
        // 1. Kiểm tra mục tiêu
        if (currentTarget == null || !currentTarget.activeInHierarchy)
        {
            // Mục tiêu chết hoặc biến mất -> Quay về Idle
            currentState = UnitState.Idle;
            mover.StopMovement();
            return;
        }

        // 2. Kiểm tra khoảng cách
        Vector3 directionVector = currentTarget.transform.position - transform.position;
        directionVector.y = 0;
        float sqrDistance = directionVector.sqrMagnitude;
        float attackRange = stats.AttackRange; // Đảm bảo Stats của bạn có biến AttackRange

        // 3. Hành động
        if (sqrDistance <= attackRange * attackRange)
        {
            // Trong tầm: Dừng lại và tấn công
            mover.StopMovement();
            transform.LookAt(currentTarget.transform); // Xoay mặt
            anim.PlaySpecialAnimation(AnimationType.Attack);
        }
        else
        {
            // Ngoài tầm: Đuổi theo
            mover.MoveTowards(currentTarget.transform.position);
        }
    }

    /// <summary>
    /// MỚI: Hàm quét tìm kẻ địch xung quanh
    /// </summary>
    private GameObject FindClosestEnemyInRange()
    {
        // Giả sử Stats của bạn có biến 'TriggerRange'
        Collider[] enemies = Physics.OverlapSphere(transform.position, stats.TriggerRange, enemyLayer);

        GameObject closestEnemy = null;
        float minSqrDistance = float.MaxValue;

        foreach (var col in enemies)
        {
            // Bỏ qua nếu collider là của chính mình (nếu lính cũng ở layer Enemy)
            if (col.transform == this.transform) continue;

            float sqrDist = (col.transform.position - transform.position).sqrMagnitude;
            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                closestEnemy = col.gameObject; // Lấy GameObject cha
            }
        }
        return closestEnemy;
    }


    // --- CÁC HÀM NHẬN LỆNH TỪ UNIT MANAGER ---
    // (Giữ nguyên)

    public void Select()
    {
       
        health.ShowHealthBar();
    }

    public void Deselect()
    {
     
        health.HideHealthBar();
    }

    public void ReceiveMoveCommand(Vector3 destination)
    {
        currentState = UnitState.Moving; // Chuyển sang di chuyển
        currentTarget = null; // QUAN TRỌNG: Hủy lệnh tấn công (cả tự động và thủ công)

        PathNode startNode = PathNodeManager.Instance.FindClosestNode(transform.position);
        PathNode endNode = PathNodeManager.Instance.FindClosestNode(destination);
        if (startNode == null || endNode == null) return;

        List<PathNode> newPath = pathFinder.FindPath(startNode, endNode);
        mover.SetPath(newPath, destination);
    }

    public void ReceiveAttackCommand(GameObject target)
    {
        currentState = UnitState.Attacking; // Chuyển sang tấn công
        currentTarget = target; // Gán mục tiêu thủ công
    }
}