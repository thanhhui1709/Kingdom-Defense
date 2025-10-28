using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Stats), typeof(MovementController))]
public class CombatAI : MonoBehaviour
{
    // Các trạng thái của AI
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

    private HashSet<GameObject> tempSet=new HashSet<GameObject>();

    // Sử dụng HashSet để tối ưu việc thêm/xóa
    private HashSet<GameObject> inRangeTargets = new HashSet<GameObject>();

    void Awake()
    {
        stats = GetComponent<Stats>();
        mover = GetComponent<MovementController>();
        anim = GetComponent<AnimationController>();
    }

    void Start()
    {
        // Trạng thái ban đầu là di chuyển theo path
        currentState = AIState.MovingOnPath;
    }

    void Update()
    {

        // 1. Cập nhật thông tin (nhìn xung quanh, kiểm tra mục tiêu)
        FindAndValidateTargets();

        // 2. Ra quyết định dựa trên thông tin
        DecideNextAction();

        // 3. Thực thi hành động dựa trên trạng thái hiện tại
        ExecuteAction();
    }

    /// <summary>
    /// Tìm kiếm và xác thực các mục tiêu xung quanh.
    /// </summary>
    private void FindAndValidateTargets()
    {
        // Xóa các mục tiêu không hợp lệ (null, bị phá hủy, ngoài tầm trigger)
        inRangeTargets.RemoveWhere(x => x == null || !x.activeInHierarchy || Vector3.Distance(transform.position, x.transform.position) > stats.TriggerRange);
        tempSet.RemoveWhere(x => x == null || !x.activeInHierarchy || Vector3.Distance(transform.position, x.transform.position) > stats.AttackRange);

        // Nếu mục tiêu hiện tại không hợp lệ (chết, ngoài tầm), xóa nó
        if (currentTarget != null)
        {
            if (!currentTarget.activeInHierarchy || Vector3.Distance(transform.position, currentTarget.transform.position) > stats.TriggerRange)
            {
                currentTarget = null;
            }
        }

        // Nếu không có mục tiêu, tìm mục tiêu mới
        if (currentTarget == null)
        {
            // Tìm các Unit và Tower mới trong tầm
            Util.FindTargetsWithChildColliders<Stats>(inRangeTargets, transform.position, stats.TriggerRange, targetLayer);

            // Ưu tiên tấn công Unit trước
            var units = inRangeTargets.Where(t => t.CompareTag("Unit")).ToList();
            if (units.Any())
            {
                currentTarget = units.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault();
            }
            else // Nếu không có Unit thì tìm Tower
            {
                var towers = inRangeTargets.Where(t => t.CompareTag("Tower")).ToList();
                if (towers.Any())
                {
                    Debug.Log("targeting Tower.");
                    currentTarget = towers.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault();
                }
            }
        }
    }

    /// <summary>
    /// Quyết định trạng thái tiếp theo dựa trên mục tiêu hiện tại.
    /// </summary>
    // Trong class CombatAI.cs, thay thế hàm DecideNextAction() cũ bằng hàm này.

    /// <summary>
    /// Quyết định trạng thái tiếp theo dựa trên mục tiêu hiện tại.
    /// </summary>
    private void DecideNextAction()
    {
        if (currentTarget == null)
        {
            // MỚI: KIỂM TRA TRẠNG THÁI TRƯỚC ĐÓ
            // Nếu trước đó đang đuổi theo hoặc tấn công, tức là vừa kết thúc giao tranh.
            // Lúc này cần cập nhật lại vị trí trên path trước khi tiếp tục.
            if (currentState == AIState.ChasingTarget || currentState == AIState.Attacking)
            {
                mover.UpdatePathToClosestNode();
            }

            currentState = AIState.MovingOnPath;
            return;
        }

        Debug.Log("Target name" + currentTarget.name);
        Util.FindTargetsWithChildColliders<Stats>(tempSet, transform.position, stats.AttackRange,targetLayer);
        if(tempSet.Count > 0)
            Debug.Log("Found target in attack range: " + tempSet.First().name);
        // So sánh với bình phương tầm đánh
        if (tempSet.Contains(currentTarget))
        {
            currentState = AIState.Attacking;
        }
        else
        {
            currentState = AIState.ChasingTarget;
        }
    }

    /// <summary>
    /// Thực thi hành động tương ứng với trạng thái.
    /// </summary>
    private void ExecuteAction()
    {
        switch (currentState)
        {
            case AIState.MovingOnPath:
                mover.ResumePathMovement(); // Yêu cầu di chuyển theo path
                break;

            case AIState.ChasingTarget:
                mover.MoveTowards(currentTarget.transform.position); // Yêu cầu di chuyển tới mục tiêu
                break;

            case AIState.Attacking:
                mover.StopMovement(); // Dừng di chuyển
                AttackTarget();
                break;
        }
    }

    private void AttackTarget()
    {
        // TODO: Viết logic tấn công của bạn ở đây
        // Ví dụ: quay mặt về mục tiêu, chạy animation tấn công, tạo ra đạn...
        transform.LookAt(currentTarget.transform);
     
        anim.PlaySpecialAnimation(AnimationType.Attack); 
        Debug.Log("Attacking " + currentTarget.name);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats.TriggerRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.AttackRange);
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}