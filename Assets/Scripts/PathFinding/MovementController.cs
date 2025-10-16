using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Stats))]
public class MovementController : MonoBehaviour
{
    public PathNode startNode;
    public PathNode endNode;

    private List<PathNode> path;
    private Stats stats;
    private Rigidbody rb;

    private int currentPathIndex;
    private bool isMovingOnPath = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<Stats>();
    }

    void Start()
    {
        PathFinding pathFinding = new PathFinding();
        path = pathFinding.FindPath(startNode, endNode);

        rb.freezeRotation = true;

        if (path == null || path.Count < 2)
        {
            Debug.LogWarning("Path is null or too short.", this);
            path = null;
        }
        else
        {
            // Bắt đầu từ điểm đầu tiên trên đường đi
            currentPathIndex = 0;
        }
    }

    void FixedUpdate()
    {
        // Chỉ di chuyển theo path khi được lệnh
        if (isMovingOnPath)
        {
            UpdatePathMovement();
        }
    }

    //-----------------------------------------------------
    // CÁC HÀM NHẬN LỆNH TỪ BÊN NGOÀI (COMBAT AI)
    //-----------------------------------------------------

    /// <summary>
    /// Lệnh: Di chuyển tới một vị trí cụ thể (mục tiêu).
    /// </summary>
    public void MoveTowards(Vector3 targetPosition)
    {
        isMovingOnPath = false; // Ngừng di chuyển theo path

        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Chỉ di chuyển trên mặt phẳng XZ

        Vector3 newPosition = rb.position + direction * stats.MoveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // Quay mặt về phía mục tiêu
        transform.LookAt(targetPosition);
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
    // LOGIC NỘI BỘ
    //-----------------------------------------------------

    private void UpdatePathMovement()
    {
        if (path == null || currentPathIndex >= path.Count)
        {
            StopMovement();
            return;
        }

        PathNode currentTargetNode = path[currentPathIndex];
        Vector3 targetPos = currentTargetNode.transform.position;

        // Nếu đã đến gần node, chuyển sang node tiếp theo
        if (Vector3.Distance(transform.position, targetPos) < 0.5f)
        {
            currentPathIndex++;
            if (currentPathIndex >= path.Count)
            {
                Debug.Log("Reached final destination on path!");
                StopMovement();
                return;
            }
        }

        // Di chuyển tới node hiện tại
        MoveTowards(targetPos);
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