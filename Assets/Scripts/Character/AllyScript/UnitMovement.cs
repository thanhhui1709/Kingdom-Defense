using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Stats))]
public class UnitMovement : MonoBehaviour
{
    private List<PathNode> path;
    private Stats stats;
    private Rigidbody rb;

    private int currentPathIndex;
    private bool isMovingOnPath = false;
    private Vector3 finalDestination;
    private AnimationController animationController;

    // --- THÊM MỚI ---
    // Biến này sẽ được "não" (Unit.cs) đọc
    public float CurrentSpeed { get; private set; }
    // --- KẾT THÚC THÊM MỚI ---

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

            // --- SỬA LỖI ---
            CurrentSpeed = horizontalVelocity.magnitude; // Cập nhật tốc độ
            animationController.Play(AnimationType.Walk, CurrentSpeed);
            // --- KẾT THÚC SỬA ---
        }
        else
        {
            animationController.Play(AnimationType.Walk, 0);

            // --- THÊM MỚI ---
            CurrentSpeed = 0f; // Đảm bảo tốc độ là 0 khi đứng yên
            // --- KẾT THÚC THÊM MỚI ---
        }
    }

    // (Các hàm SetPath, MoveTowards giữ nguyên)
    public void SetPath(List<PathNode> newPath, Vector3 finalClickPosition)
    {
        this.finalDestination = finalClickPosition;

        if (newPath == null || newPath.Count == 0)
        {
            path = null;
        }
        else if (newPath.Count == 1)
        {
            path = newPath;
            currentPathIndex = 0;
        }
        else
        {
            path = newPath;
            currentPathIndex = 1; // BỎ QUA node đầu tiên
        }

        isMovingOnPath = true;
    }

    public void MoveTowards(Vector3 targetPosition)
    {
        isMovingOnPath = false;
        InternalMoveTowards(targetPosition);
    }

    // --- SỬA LẠI HÀM NÀY ---
    public void StopMovement()
    {
        isMovingOnPath = false;
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        CurrentSpeed = 0f; // Đặt tốc độ về 0
    }

    public bool IsMoving()
    {
        return isMovingOnPath;
    }

    // (UpdatePathMovement, MoveTowardsPosition, InternalMoveTowards giữ nguyên)
    private void UpdatePathMovement()
    {
        if (path == null)
        {
            if (MoveTowardsPosition(finalDestination))
            {
                StopMovement();
            }
            return;
        }

        if (currentPathIndex >= path.Count)
        {
            StopMovement();
            return;
        }

        Vector3 targetPos;
        bool isFinalNodeInPath = (currentPathIndex == path.Count - 1);

        if (isFinalNodeInPath)
        {
            targetPos = finalDestination;
        }
        else
        {
            targetPos = path[currentPathIndex].transform.position;
        }

        if (MoveTowardsPosition(targetPos))
        {
            if (isFinalNodeInPath)
            {
                StopMovement();
                return;
            }
            else
            {
                currentPathIndex++;
            }
        }
    }

    private bool MoveTowardsPosition(Vector3 targetPosition)
    {
        InternalMoveTowards(targetPosition);
        float distanceThreshold = 0.5f;
        if ((transform.position - targetPosition).sqrMagnitude < distanceThreshold * distanceThreshold)
        {
            return true; // Đã đến
        }
        return false; // Vẫn đang di chuyển
    }

    private void InternalMoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        Vector3 targetVelocity = direction * stats.MoveSpeed;
        targetVelocity.y = rb.linearVelocity.y; // Giữ trọng lực
        rb.linearVelocity = targetVelocity;

        Vector3 lookTarget = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        transform.LookAt(lookTarget);
    }
}