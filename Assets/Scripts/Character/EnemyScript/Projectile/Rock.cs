using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rock : MonoBehaviour, IProjectile
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float pushForce;
    public LayerMask layer;
    public AudioClip hitSound;
    private GameObject target;
    private float damage;
    private Rigidbody rb;

    public void Launch(List<GameObject> target, float damage)
    {

        // 1. Xử lý trường hợp không có mục tiêu
        if (target == null || target.Count == 0)
        {
            ObjectPoolManager.ReturnObject(gameObject); // Không có mục tiêu, tự hủy
            return;
        }

        GameObject closestTarget = null;
        float minSqrDistance = float.MaxValue; // Bắt đầu bằng vô cực
        Vector3 currentPos = transform.position; // Lưu vị trí của đạn

        // 2. Lặp qua danh sách MỘT LẦN (O(n))
        foreach (GameObject t in target)
        {
            // Bỏ qua nếu mục tiêu không hợp lệ
            if (t == null || !t.activeInHierarchy)
                continue;

            // 3. Tối ưu: Dùng SqrDistance (bỏ qua phép căn bậc hai)
            float sqrDistance = Vector3.Distance(t.transform.position, currentPos);

            // 4. So sánh
            if (sqrDistance < minSqrDistance)
            {
                // Tìm thấy mục tiêu mới gần hơn
                minSqrDistance = sqrDistance;
                closestTarget = t;
            }
        }

        // 5. Gán kết quả
        this.target = closestTarget;
        this.damage = damage;

        InvokeRepeating(nameof(CheckTargetAlive), 0.2f, 0.25f);

        // (Nếu lặp xong vẫn không tìm thấy mục tiêu hợp lệ)
        if (this.target == null)
        {
            ObjectPoolManager.ReturnObject(gameObject);
        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == layer)
        {
            IHealthSystem health = other.GetComponent<IHealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damage);

            }
            Rigidbody otherRb = other.GetComponent<Rigidbody>();
            if (otherRb != null)
            {
                Vector3 forceDirection = (other.transform.position - transform.position).normalized;
                otherRb.AddForce(forceDirection * pushForce, ForceMode.Impulse);
            }
            ObjectPoolManager.ReturnObject(gameObject);
        }
    }
    private void CheckTargetAlive()
    {
        IHealthSystem health = target.GetComponent<IHealthSystem>();
        if (target == null || !target.activeInHierarchy || health.HasDie())
        {
            ObjectPoolManager.ReturnObject(gameObject);
        }

    }
}
