// File: Scripts/Character/EnemyScript/PoisonSlowEffect.cs

using UnityEngine;

[RequireComponent(typeof(CircularMovement))]
public class PoisonSlowEffect : MonoBehaviour
{
    // Các thông số vẫn giữ nguyên
    public float duration = 1f;
    public float slowFactor = 0.5f;
    public Color poisonColor = Color.red;

    // --- THAY ĐỔI Ở ĐÂY ---
    // Chúng ta dùng "Renderer" thay vì "SpriteRenderer"
    // "Renderer" là lớp cha của cả MeshRenderer (3D) và SpriteRenderer (2D)
    private Renderer objectRenderer;
    private CircularMovement circularMovement;

    private float originalSpeed;
    private Color originalColor;
    private float effectTimer;

    void Start()
    {
        circularMovement = GetComponent<CircularMovement>();

        // --- THAY ĐỔI Ở ĐÂY ---
        // Tìm bất kỳ component Renderer nào (MeshRenderer hoặc SpriteRenderer) trong đối tượng con
        objectRenderer = GetComponentInChildren<Renderer>();

        if (objectRenderer == null)
        {
            // Báo lỗi rõ ràng hơn
            Debug.LogError("Không tìm thấy MeshRenderer hoặc SpriteRenderer trên đối tượng: " + gameObject.name);
            Destroy(this); // Tự hủy script nếu không tìm thấy
            return;
        }

        // Lưu lại tốc độ hiện tại
        originalSpeed = circularMovement.currentSpeed;

        // --- THAY ĐỔI Ở ĐÂY ---
        // Lấy màu từ "material" của Renderer
        originalColor = objectRenderer.material.color;

        // Áp dụng hiệu ứng
        ApplyEffect();

        // Bắt đầu đếm ngược
        effectTimer = duration;
    }

    void Update()
    {
        effectTimer -= Time.deltaTime;
        if (effectTimer <= 0)
        {
            RemoveEffect();
        }
    }

    private void ApplyEffect()
    {
        // Làm chậm (giữ nguyên)
        circularMovement.currentSpeed = originalSpeed * (1 - slowFactor);

        // --- THAY ĐỔI Ở ĐÂY ---
        // Đổi màu trên "material" của Renderer
        objectRenderer.material.color = poisonColor;
    }

    private void RemoveEffect()
    {
        if (circularMovement != null)
        {
            circularMovement.currentSpeed = originalSpeed;
        }

        // --- THAY ĐỔI Ở ĐÂY ---
        // Kiểm tra xem renderer còn tồn tại không trước khi khôi phục màu
        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }

        Destroy(this);
    }

    public void RefreshDuration()
    {
        effectTimer = duration;
    }
}