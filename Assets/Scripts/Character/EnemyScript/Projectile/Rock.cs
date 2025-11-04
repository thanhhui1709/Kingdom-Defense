using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rock : MonoBehaviour, IProjectile
{
    [Header("AOE Settings")]
    public float explosionRadius = 2f;   // Bán kính vùng nổ
    public float explosionForce = 5f;    // Lực đẩy lan
    [SerializeField]
    private float speed = 5f;
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
    private void FixedUpdate()
    {
        if (target == null) return;
        Util.MoveToward(rb, target.transform, speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ nổ khi chạm đúng layer chỉ định
        if ((layer.value & (1 << other.gameObject.layer)) == 0)
            return;

        // --- TẠO VÙNG HIỆU LỰC ---
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, layer);

        foreach (Collider hit in hits)
        {
            IHealthSystem healthCheck = hit.GetComponentInParent<IHealthSystem>();
            // Gây damage
            if (healthCheck!=null)
            {
                healthCheck.TakeDamage(damage);
            }

            Rigidbody hitRb=hit.GetComponentInParent<Rigidbody>();
            // Đẩy lùi
            if (hitRb!=null)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
               hitRb.AddForce(dir * explosionForce, ForceMode.Impulse);
            }
        }

        // ✨ Option: hiệu ứng vụ nổ (nếu có poolFX thì dùng, không thì bỏ)
        // ObjectPoolManager.SpawnObject(explosionEffectPrefab, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Particle);

        // Trả đá về pool
        ObjectPoolManager.ReturnObject(gameObject);
    }

    private void CheckTargetAlive()
    {
        IHealthSystem health = target.GetComponent<IHealthSystem>();
        if (target == null || !target.activeInHierarchy || health.HasDie())
        {
            ObjectPoolManager.ReturnObject(gameObject);
        }

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        StartCoroutine(KnockbackCoroutine(direction, force, duration));
    }

    private IEnumerator KnockbackCoroutine(Vector3 direction, float force, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            rb.MovePosition(rb.position + force * Time.deltaTime * direction);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
