using UnityEngine;

// Tên class đã được đổi thành "Style360Spawner" vì C# không cho phép tên bắt đầu bằng số.
// Bạn hãy chắc chắn rằng tên file script cũng là "Style360Spawner.cs".
public class Style360Spawner : MonoBehaviour
{
    [Header("Cấu hình Object")]
    // 1. CÁI GÌ: Kéo Prefab của lính (hoặc đạn) vào đây
    public GameObject objectToSpawn;

    [Header("Cấu hình Vòng Tròn")]
    // 2. BAO NHIÊU: Số lượng lính sẽ được tạo ra trong một lần
    public int numberOfObjects = 12;

    // 3. Ở ĐÂU: Bán kính của vòng tròn spawn
    public float radius = 5f;

    [Header("Điều khiển")]
    // 4. KHI NÀO: Phím bấm để kích hoạt spawn
    public KeyCode spawnKey = KeyCode.Space;

    // Update được gọi mỗi frame
    void Update()
    {
        // Kiểm tra xem người chơi có nhấn phím đã định (mặc định là phím Space) không
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnObjectsInCircle();
        }
    }

    /// <summary>
    /// Hàm chính để tạo ra các đối tượng theo một vòng tròn.
    /// </summary>
    void SpawnObjectsInCircle()
    {
        // Lấy vị trí trung tâm, chính là vị trí của đối tượng Spawner này
        Vector2 centerPosition = transform.position;

        // Tính toán góc giữa mỗi đối tượng. Một vòng tròn đầy đủ là 360 độ.
        float angleStep = 360f / numberOfObjects;
        float currentAngle = 0f;

        // Bắt đầu vòng lặp để tạo ra từng đối tượng
        for (int i = 0; i < numberOfObjects; i++)
        {
            // Tính toán vị trí (x, y) trên vòng tròn bằng lượng giác
            // Chuyển đổi góc từ độ (degrees) sang radian vì các hàm Sin/Cos yêu cầu radian
            float angleInRad = currentAngle * Mathf.Deg2Rad;

            // Tính toán tọa độ x và y dựa trên góc và bán kính
            float x = centerPosition.x + radius * Mathf.Cos(angleInRad);
            float y = centerPosition.y + radius * Mathf.Sin(angleInRad);

            Vector2 spawnPosition = new Vector2(x, y);

            // Tạo (Instantiate) đối tượng từ Prefab tại vị trí vừa tính toán
            // Quaternion.identity nghĩa là không xoay đối tượng
            Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);

            // Tăng góc cho đối tượng tiếp theo
            currentAngle += angleStep;
        }

        Debug.Log($"Đã spawn {numberOfObjects} đối tượng theo vòng tròn!");
    }
}