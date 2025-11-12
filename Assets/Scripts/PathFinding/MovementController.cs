using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Stats))]
public class MovementController : MonoBehaviour
{
  

    private List<PathNode> path; // Vẫn dùng để lưu đường đi được truyền vào
    private Stats stats;
    private Rigidbody rb;

    private int currentPathIndex;
    private bool isMovingOnPath = false;
    private AnimationController anim;
    private Vector3 movementOffset; // Offset ngẫu nhiên, cố định
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<Stats>();
        anim = GetComponent<AnimationController>();
        movementOffset = Random.insideUnitSphere * 2.5f;
        movementOffset.y = 0; // Chỉ làm phẳng
    }

    void Start()
    {
        

        rb.freezeRotation = true;

    }

    void FixedUpdate()
    {
        // Chỉ di chuyển theo path khi được lệnh
        if (isMovingOnPath)
        {
            UpdatePathMovement();
        }
    }
    public bool IsMovingOnPath()
    {
        return isMovingOnPath;
    }

    //-----------------------------------------------------
    // CÁC HÀM NHẬN LỆNH TỪ BÊN NGOÀI
    //-----------------------------------------------------

    /// <summary>
    /// MỚI: Hàm công khai để nhận một đường đi mới từ bên ngoài.
    /// </summary>
    /// <param name="newPath">Danh sách các PathNode để di chuyển theo.</param>
    public void SetPath(List<PathNode> newPath)
    {
        if (newPath == null || newPath.Count == 0)
        {
            Debug.LogWarning("Một đường đi rỗng hoặc null đã được gán.", this);
            path = null;
            StopMovement();
            return;
        }

        path = newPath;
        currentPathIndex = 0; // Luôn bắt đầu từ đầu của đường đi mới
        ResumePathMovement(); // Bắt đầu di chuyển ngay lập tức
    }

    /// <summary>
    /// Lệnh: Di chuyển tới một vị trí cụ thể (mục tiêu).
    /// </summary>
    public void MoveTowards(Vector3 targetPosition)
    {
        isMovingOnPath = false; // Ngừng di chuyển theo path
        Vector3 offsetTargetPosition = targetPosition + movementOffset;
        Vector3 direction = (offsetTargetPosition - transform.position).normalized;
        direction.y = 0;

        Vector3 newPosition = rb.position + direction * stats.MoveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // Quay mặt về phía mục tiêu (vẫn quay mặt vào mục tiêu GỐC)
        transform.LookAt(targetPosition);
        anim.Play(AnimationType.Walk, stats.MoveSpeed);
    }

    /// <summary>
    /// Lệnh: Tiếp tục di chuyển theo path đã định.
    /// </summary>
    public void ResumePathMovement()
    {
        isMovingOnPath = true;
    }

    /// <summary>
    /// Lệnh: Dừng mọi chuyển động.
    /// </summary>
    public void StopMovement()
    {
        isMovingOnPath = false;
    }

    //-----------------------------------------------------
    // LOGIC NỘI BỘ (Giữ nguyên)
    //-----------------------------------------------------

    private void UpdatePathMovement()
    {
        if (path == null || currentPathIndex >= path.Count)
        {
            StopMovement(); // <-- Dòng này sẽ set isMovingOnPath = false
            return;
        }

        PathNode currentTargetNode = path[currentPathIndex];
        Vector3 targetPos = currentTargetNode.transform.position;

        // Nếu đã đến gần node, chuyển sang node tiếp theo
        // Sử dụng bình phương khoảng cách để tối ưu (nhanh hơn)
        float distanceThreshold = 0.5f;
        if ((transform.position - targetPos).sqrMagnitude < distanceThreshold * distanceThreshold)
        {
            currentPathIndex++;
            if (currentPathIndex >= path.Count)
            {
                Debug.Log("Reached final destination on path!");
                StopMovement();
                // TODO: Gửi sự kiện đến đích (ví dụ: gây sát thương cho nhà chính)
                return;
            }
        }

        // Di chuyển tới node hiện tại
        // Lấy lại targetPos phòng trường hợp index vừa thay đổi
        targetPos = path[currentPathIndex].transform.position;

        // Gọi phiên bản di chuyển nội bộ để tránh set isMovingOnPath = false
        InternalMoveTowards(targetPos);
    }

   
    private void InternalMoveTowards(Vector3 targetPosition)
    {
        // 1. Tính toán hướng di chuyển trên mặt phẳng XZ
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        // 2. Tạo vận tốc mục tiêu
        Vector3 targetVelocity = direction * stats.MoveSpeed;

        // 3. QUAN TRỌNG: Giữ nguyên vận tốc của trục Y
        // Điều này cho phép trọng lực (gravity) kéo nhân vật xuống mặt đất
        targetVelocity.y = rb.linearVelocity.y;

        // 4. Gán vận tốc mới
        // Thay vì MovePosition, chúng ta dùng linearVelocity
        rb.linearVelocity = targetVelocity;

        // 5. Quay mặt về phía mục tiêu
        transform.LookAt(targetPosition);
        anim.Play(AnimationType.Walk, stats.MoveSpeed);
    }

    /// <summary>
    /// MỚI: Tìm node gần nhất trên path so với vị trí hiện tại và cập nhật lại mục tiêu.
    /// </summary>
    public void UpdatePathToClosestNode()
    {
        if (path == null || path.Count == 0) return;

        int closestNodeIndex = -1;
        float minDistance = float.MaxValue;

        // Tối ưu: Chỉ tìm kiếm từ node hiện tại trở đi.
        // Điều này ngăn AI đi ngược lại trên con đường nó đã qua.
        for (int i = currentPathIndex; i < path.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, path[i].transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNodeIndex = i;
            }
        }

        if (closestNodeIndex != -1)
        {
            // Cập nhật lại index để lần di chuyển tiếp theo sẽ đi từ đây
            currentPathIndex = closestNodeIndex;
            Debug.Log("Path updated. Resuming from new closest node: " + path[currentPathIndex].name);
        }
    }
}