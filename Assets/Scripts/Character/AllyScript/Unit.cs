using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MovementController), typeof(Stats), typeof(Collider))]
public class Unit : MonoBehaviour
{
    // Trạng thái của lính
    private enum UnitState
    {
        Idle,       // Đứng yên chờ lệnh
        Moving,     // Đang di chuyển theo đường đi (path)
        Attacking   // Đang đuổi theo/tấn công mục tiêu
    }

    // Components
    private MovementController mover;
    private Stats stats;
    // private AnimationController animController; // (Tùy chọn: nếu bạn có)
    private PathFinding pathFinder;

    [SerializeField] private GameObject selectionVisual; // Vòng tròn chọn

    // State Management
    private UnitState currentState;
    private GameObject currentTarget;

    void Awake()
    {
        mover = GetComponent<MovementController>();
        stats = GetComponent<Stats>();
        // animController = GetComponent<AnimationController>();
        pathFinder = new PathFinding();

        if (selectionVisual != null) selectionVisual.SetActive(false);
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
        // Bộ não của lính chạy mỗi frame
        switch (currentState)
        {
            case UnitState.Idle:
                // Không làm gì, chờ lệnh từ UnitManager
                // animController?.Play(AnimationType.Walk, false);
                break;

            case UnitState.Moving:
                // Kiểm tra xem MovementController đã đi hết đường chưa
                if (!mover.IsMovingOnPath())
                {
                    currentState = UnitState.Idle;
                    // animController?.Play(AnimationType.Walk, false);
                }
                break;

            case UnitState.Attacking:
                HandleAttackingState();
                break;
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
            currentState = UnitState.Idle;
            // animController?.Play(AnimationType.Walk, false);
            mover.StopMovement();
            return;
        }

        // 2. Kiểm tra khoảng cách (dùng logic 2D nhanh)
        Vector3 directionVector = currentTarget.transform.position - transform.position;
        directionVector.y = 0;
        float sqrDistance = directionVector.sqrMagnitude;
        float attackRange = stats.AttackRange; // Đảm bảo Stats của bạn có biến AttackRange

        // 3. Hành động dựa trên khoảng cách
        if (sqrDistance <= attackRange * attackRange)
        {
            // Trong tầm: Dừng lại và tấn công
            mover.StopMovement();
            transform.LookAt(currentTarget.transform); // Xoay mặt về mục tiêu

            // animController?.Play(AnimationType.Walk, false);
            // animController?.Play(AnimationType.Attack); 
            // TODO: Thêm logic cooldown tấn công của riêng lính ở đây
        }
        else
        {
            // Ngoài tầm: Đuổi theo
            mover.MoveTowards(currentTarget.transform.position);
            // animController?.Play(AnimationType.Walk, true);
        }
    }

    // --- CÁC HÀM NHẬN LỆNH TỪ UNIT MANAGER ---

    public void Select()
    {
        if (selectionVisual != null) selectionVisual.SetActive(true);
    }

    public void Deselect()
    {
        if (selectionVisual != null) selectionVisual.SetActive(false);
    }

    /// <summary>
    /// Nhận lệnh di chuyển tới 1 vị trí (Vector3)
    /// </summary>
    public void ReceiveMoveCommand(Vector3 destination)
    {
        currentState = UnitState.Moving; // Chuyển sang trạng thái Di chuyển
        currentTarget = null; // Hủy mọi lệnh tấn công cũ

        // Tìm đường đi
        PathNode startNode = PathNodeManager.Instance.FindClosestNode(transform.position);
        PathNode endNode = PathNodeManager.Instance.FindClosestNode(destination);

        if (startNode == null || endNode == null) return;

        List<PathNode> newPath = pathFinder.FindPath(startNode, endNode);

        // Giao đường đi cho "chân" (MovementController)
        mover.SetPath(newPath);
        // animController?.Play(AnimationType.Walk, true);
    }

    /// <summary>
    /// Nhận lệnh tấn công 1 mục tiêu
    /// </summary>
    public void ReceiveAttackCommand(GameObject target)
    {
        currentState = UnitState.Attacking; // Chuyển sang trạng thái Tấn công
        currentTarget = target; // Lưu mục tiêu
    }
}