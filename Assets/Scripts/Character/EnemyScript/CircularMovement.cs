// File: Scripts/Character/EnemyScript/CIrcularMovment.cs

using UnityEngine;

[RequireComponent(typeof(Stats))] // Bắt buộc lính phải có component Stats
public class CircularMovement : MonoBehaviour
{
    [Header("Cài đặt di chuyển")]
    [Tooltip("Bán kính của vòng tròn di chuyển.")]
    public float radius = 5f;

    [Tooltip("Nếu được chọn, lính sẽ luôn nhìn về phía trước theo hướng di chuyển.")]
    public bool lookForward = true;

    [Header("Trạng thái hiện tại")]
    [Tooltip("Tốc độ di chuyển thực tế, sẽ bị thay đổi bởi các hiệu ứng.")]
    public float currentSpeed; // Đây là biến mà hiệu ứng độc sẽ thay đổi

    private Stats stats;
    private Vector3 centerPoint;
    private float angle;

    void Awake()
    {
        stats = GetComponent<Stats>();
    }

    void Start()
    {
        // Gán tốc độ di chuyển ban đầu từ chỉ số gốc trong Stats
        currentSpeed = stats.moveSpeed;

        // Lưu vị trí ban đầu làm tâm của vòng tròn
        centerPoint = transform.position;
    }

    void Update()
    {
        // Di chuyển dựa trên tốc độ HIỆN TẠI (currentSpeed)
        angle += currentSpeed * Time.deltaTime;

        // Tính toán vị trí mới trên vòng tròn
        float x = centerPoint.x + radius * Mathf.Cos(angle);
        float z = centerPoint.z + radius * Mathf.Sin(angle);
        float y = centerPoint.y;
        Vector3 newPosition = new Vector3(x, y, z);

        // Xoay mặt về hướng di chuyển
        if (lookForward && newPosition != transform.position)
        {
            Vector3 direction = newPosition - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }

        // Cập nhật vị trí
        transform.position = newPosition;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = (Application.isPlaying) ? centerPoint : transform.position;
        Gizmos.DrawWireSphere(center, radius);
    }
}