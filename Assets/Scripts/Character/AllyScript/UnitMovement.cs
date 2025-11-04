using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    private List<PathNode> path;
    private Stats stats;
    private Rigidbody rb;

    private int currentPathIndex;
    private bool isMovingOnPath = false;
    private Vector3 finalDestination; // Đích cuối cùng (vị trí click)
    private AnimationController animationController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<Stats>();
        animationController = GetComponent<AnimationController>();  
    }

    void Start()
    {
        rb.freezeRotation = true;
   
    }

    void FixedUpdate()
    {
        if (isMovingOnPath)
        {
            UpdatePathMovement();
        
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            float currentSpeed = horizontalVelocity.magnitude;

            animationController.Play(AnimationType.Walk, currentSpeed);
            Debug.Log("Current Speed: " + currentSpeed);
        }
        else
        {
            animationController.Play(AnimationType.Walk, 0);
        }
    }

    //-----------------------------------------------------
    // HÀM NHẬN LỆNH (Từ Unit.cs)
    //-----------------------------------------------------

    /// <summary>
    /// Nhận đường đi và 1 điểm click chính xác.
    /// Sẽ tự động bỏ qua node đầu tiên.
    /// </summary>
    public void SetPath(List<PathNode> newPath, Vector3 finalClickPosition)
    {
        this.finalDestination = finalClickPosition; // Lưu vị trí click

        if (newPath == null || newPath.Count == 0)
        {
            path = null; // Không có path, sẽ đi thẳng
        }
        else if (newPath.Count == 1)
        {
            path = newPath;
            currentPathIndex = 0; // Click gần, đi tới tâm
        }
        else
        {
            path = newPath;
            currentPathIndex = 1; // BỎ QUA node đầu tiên
        }

        isMovingOnPath = true; // Bắt đầu di chuyển
    }

    /// <summary>
    /// Dành cho Unit.cs gọi khi đuổi theo mục tiêu
    /// </summary>
    public void MoveTowards(Vector3 targetPosition)
    {
        isMovingOnPath = false; // Ngừng di chuyển theo path
        InternalMoveTowards(targetPosition);
    }

    /// <summary>
    /// Dừng mọi chuyển động
    /// </summary>
    public void StopMovement()
    {
        isMovingOnPath = false;
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
      
    }

    /// <summary>
    /// Hàm để Unit.cs (não) biết khi nào đã đi hết đường
    /// </summary>
    public bool IsMoving()
    {
        return isMovingOnPath;
    }

    //-----------------------------------------------------
    // LOGIC NỘI BỘ
    //-----------------------------------------------------

    private void UpdatePathMovement()
    {
        // Trường hợp 1: Không có đường đi (click gần) -> Đi thẳng tới đích
        if (path == null)
        {
            if (MoveTowardsPosition(finalDestination))
            {
                StopMovement(); // Tới nơi, dừng lại
            }
            return;
        }

        // Trường hợp 2: Có đường đi
        if (currentPathIndex >= path.Count)
        {
            StopMovement();
            return;
        }

        Vector3 targetPos;
        bool isFinalNodeInPath = (currentPathIndex == path.Count - 1);

        if (isFinalNodeInPath)
        {
            // Nếu là node cuối, mục tiêu là vị trí click CHÍNH XÁC
            targetPos = finalDestination;
        }
        else
        {
            // Nếu là node trung gian, mục tiêu là TÂM của node
            targetPos = path[currentPathIndex].transform.position;
        }

        // Kiểm tra xem đã đến gần mục tiêu chưa
        if (MoveTowardsPosition(targetPos))
        {
            // Đã đến gần targetPos
            if (isFinalNodeInPath)
            {
                // Tới nơi rồi
                StopMovement();
                return;
            }
            else
            {
                // Tới node trung gian, chuyển sang node tiếp theo
                currentPathIndex++;
            }
        }
    }

    /// <summary>
    /// Di chuyển tới 1 vị trí và trả về TRUE nếu đã đến nơi
    /// </summary>
    private bool MoveTowardsPosition(Vector3 targetPosition)
    {
        InternalMoveTowards(targetPosition);

        // Kiểm tra khoảng cách
        float distanceThreshold = 0.5f;
        if ((transform.position - targetPosition).sqrMagnitude < distanceThreshold * distanceThreshold)
        {
            return true; // Đã đến
        }
        return false; // Vẫn đang di chuyển
    }

    /// <summary>
    /// Hàm di chuyển vật lý
    /// </summary>
    private void InternalMoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        Vector3 targetVelocity = direction * stats.MoveSpeed;
        targetVelocity.y = rb.linearVelocity.y; // Giữ trọng lực
        rb.linearVelocity = targetVelocity;
      

        // Quay mặt về phía mục tiêu
        Vector3 lookTarget = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        transform.LookAt(lookTarget);
    }
}
