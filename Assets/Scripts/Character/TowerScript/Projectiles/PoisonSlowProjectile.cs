using System.Collections.Generic;
using UnityEngine;

public class PoisonSlowProjectile : MonoBehaviour, IProjectile
{
    [Header("Thông số đạn")]
    public float speed = 20f;
    private Transform target;

    [Header("Thông số hiệu ứng")]
    public float effectDuration = 1f;
    [Range(0, 1)]
    public float slowFactor = 0.3f;
    public Color debuffColor = Color.green;

    public void Launch(Transform launchPoint, List<GameObject> targets)
    {
        if (targets != null && targets.Count > 0 && targets[0] != null)
        {
            this.target = targets[0].transform;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Nếu mục tiêu biến mất giữa chừng, tự hủy
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // --- LOGIC DI CHUYỂN KHÔNG THAY ĐỔI ---
        // Vẫn di chuyển về phía mục tiêu
        Vector3 direction = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;
        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target);

        // --- XÓA PHẦN KIỂM TRA VA CHẠM CŨ ---
        // Chúng ta không còn dùng logic "if (direction.magnitude <= distanceThisFrame)" nữa.
        // Việc xử lý va chạm sẽ do hàm OnTriggerEnter đảm nhiệm.
    }

    // --- THÊM HÀM MỚI NÀY ---
    // Hàm này sẽ tự động được Unity gọi khi Collider (đã tick Is Trigger) của viên đạn
    // va chạm với một Collider khác.
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem đối tượng va chạm có phải là kẻ địch không
        if (other.CompareTag("Enemy"))
        {
            // Lấy component PoisonSlowEffect trên kẻ địch
            PoisonSlowEffect existingEffect = other.GetComponent<PoisonSlowEffect>();

            if (existingEffect != null)
            {
                // Nếu đã có, chỉ làm mới thời gian
                existingEffect.RefreshDuration();
            }
            else
            {
                // Nếu chưa có, thêm mới và gán thông số
                PoisonSlowEffect newEffect = other.gameObject.AddComponent<PoisonSlowEffect>();
                newEffect.duration = this.effectDuration;
                newEffect.slowFactor = this.slowFactor;
                newEffect.poisonColor = this.debuffColor;
            }

            // SAU KHI GÂY HIỆU ỨNG, HỦY VIÊN ĐẠN NGAY LẬP TỨC
            Destroy(gameObject);
        }
    }
}