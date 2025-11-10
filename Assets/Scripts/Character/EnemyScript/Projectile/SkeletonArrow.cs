using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SkeletonArrow : MonoBehaviour, IProjectile
{
    private GameObject target;
    private IHealthSystem targetHealth;
    private Collider targetCollider;
    private Rigidbody rb;
    private float damage;
    [SerializeField] private float speed = 10f;

    // --- THÊM MỚI (TỐI ƯU HÓA) ---
    private float checkTimer;
    private const float CHECK_INTERVAL = 0.25f; // Chỉ kiểm tra 4 lần/giây
    // --- KẾT THÚC THÊM MỚI ---

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("Rigidbody component is missing from the projectile.");
    }

    // --- SỬA LẠI HÀM ONENABLE (Quan trọng) ---
    private void OnEnable()
    {
        // Reset bộ đếm khi được tái sử dụng
        checkTimer = CHECK_INTERVAL;
    }

    // --- HÀM ĐÃ SỬA (TỐI ƯU HÓA) ---
    void FixedUpdate()
    {
        // 1. Đếm lùi timer
        checkTimer -= Time.fixedDeltaTime;

        // 2. Chỉ kiểm tra nếu timer đã hết
        if (checkTimer <= 0f)
        {
            // Đặt lại timer
            checkTimer = CHECK_INTERVAL;

            // 3. Thực hiện kiểm tra (Giờ chỉ chạy 4 lần/giây)
            if (target == null || !target.activeInHierarchy || (targetHealth != null && targetHealth.HasDie()))
            {
                ObjectPoolManager.ReturnObject(gameObject);
                return; // Dừng lại
            }
        }

        // 4. Logic di chuyển (vẫn chạy mỗi frame)
        // (Chúng ta phải kiểm tra target != null một lần nữa
        // phòng trường hợp nó bị hủy bởi 'if' ở trên)
        if (target != null)
        {
            Util.MoveToward(rb, target.transform, speed);
        }
    }

    // --- (Hàm OnTriggerEnter giữ nguyên) ---
    private void OnTriggerEnter(Collider other)
    {
        if (other == targetCollider)
        {
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
            ObjectPoolManager.ReturnObject(gameObject);
        }
    }

    // --- (Hàm Launch giữ nguyên) ---
    public void Launch(List<GameObject> targets, float damage)
    {
        this.target = targets.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault();
        this.damage = damage;

        if (this.target != null)
        {
            targetCollider = this.target.GetComponentInChildren<Collider>();
            targetHealth = this.target.GetComponentInParent<IHealthSystem>();
        }
        else
        {
            ObjectPoolManager.ReturnObject(gameObject);
        }
    }
}