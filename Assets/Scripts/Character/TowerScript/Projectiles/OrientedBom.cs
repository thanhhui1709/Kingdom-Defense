using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class OrientedBom : MonoBehaviour, IProjectile
{
    private GameObject target;
    private Rigidbody rb;
    public LayerMask layer; // QUAN TRỌNG: Đây là layer của ĐỊCH

    private float damage;

    [SerializeField] private float speed = 8f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private int ignoreArmor = 10;

    // --- THÊM MỚI: Biến guard để chống nổ 2 lần ---
    private bool hasExploded = false;
    private Collider myCollider;
    // (Bạn có thể thêm MeshRenderer nếu muốn ẩn quả bom khi nổ)
    // private MeshRenderer myRenderer; 

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
        // myRenderer = GetComponent<MeshRenderer>();
        if (rb == null) Debug.LogError("Rigidbody component is missing from the bomb projectile.");
    }

    void OnEnable()
    {
        // Reset lại trạng thái cho Object Pool
        hasExploded = false;
        myCollider.enabled = true;
        // if(myRenderer != null) myRenderer.enabled = true;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        // Nếu đã nổ, dừng mọi hoạt động
        if (hasExploded) return;

        if (target == null || !target.activeInHierarchy)
        {
            // (Tùy chọn: bạn có thể cho nó bay thẳng thay vì tự hủy)
            ObjectPoolManager.ReturnObject(gameObject);
            return;
        }
        // --- KẾT THÚC SỬA ---

        Vector3 dir = Util.MoveToward(rb, target.transform, speed); // Giả sử hàm này chỉ trả về hướng
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
        rb.MoveRotation(targetRotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Nếu đã nổ rồi thì bỏ qua
        if (hasExploded) return;

        // --- TỐI ƯU LOGIC VA CHẠM ---
        // Nổ khi chạm vào mục tiêu CHỈ ĐỊNH
        // HOẶC chạm vào BẤT CỨ THỨ GÌ trên layer của địch
        bool hitTarget = (collision.gameObject == target);
        bool hitEnemyLayer = (layer.value & (1 << collision.gameObject.layer)) != 0;

        if (hitTarget || hitEnemyLayer)
        {
            Explode();
        }
        // (Bạn có thể thêm logic nổ khi va vào tường ở đây)
    }

    private void Explode()
    {
        // --- SỬA LỖI ĐA CHẠM ---
        if (hasExploded) return;
        hasExploded = true;
        // --- KẾT THÚC SỬA ---

        // Tắt quả bom (ẩn, dừng vật lý)
        myCollider.enabled = false;
        // if(myRenderer != null) myRenderer.enabled = false;
        rb.isKinematic = true;

        // Tạo hiệu ứng nổ
        if (explosionEffect != null)
        {
            ObjectPoolManager.SpawnObject(explosionEffect, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Particle);
        }

        // --- TỐI ƯU HÓA: CHỈ QUÉT LAYER CỦA ĐỊCH ---
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, layer);
        Debug.Log("Bom nổ! Đã tìm thấy " + colliders.Length + " collider địch.");

        foreach (Collider nearby in colliders)
        {
            // Dùng GetComponentInParent để tìm health an toàn hơn
            EnemyHealth health = nearby.GetComponentInParent<EnemyHealth>();

            // Thêm kiểm tra HasDie()
            if (health != null && !health.HasDie())
            {
                health.TakeDamage(damage, ignoreArmor);
                Debug.Log("SUCCESS: " + nearby.gameObject.name + " đã nhận sát thương: " + damage);
            }

            // Đẩy lùi (dùng InParent cho an toàn)
            Rigidbody rbNearby = nearby.GetComponentInParent<Rigidbody>();
            if (rbNearby != null)
            {
                rbNearby.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        // Bắt đầu coroutine để trả về pool
        StartCoroutine(WaitToReturnToPool(1f)); // Chờ 1s cho hiệu ứng nổ
    }

    IEnumerator WaitToReturnToPool(float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPoolManager.ReturnObject(gameObject);
    }

    public void Launch(List<GameObject> targets, float damage)
    {
        // --- TỐI ƯU HÓA: TÌM MỤC TIÊU BẰNG VÒNG LẶP O(n) ---
        GameObject closestTarget = null;
        float minSqrDistance = float.MaxValue;
        Vector3 currentPos = transform.position;

        foreach (GameObject t in targets)
        {
            if (t == null || !t.activeInHierarchy) continue;

            float sqrDist = (t.transform.position - currentPos).sqrMagnitude;
            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                closestTarget = t;
            }
        }

        this.target = closestTarget;
        // --- KẾT THÚC TỐI ƯU ---

        this.damage = damage;

        // Nếu không tìm thấy mục tiêu hợp lệ, tự hủy
        if (this.target == null)
        {
            ObjectPoolManager.ReturnObject(gameObject);
            return;
        }

        Debug.Log("Bom đã được Launch với sát thương được thiết lập: " + this.damage);
    }
   
}