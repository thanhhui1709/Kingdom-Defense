using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class ExplosionArrow : MonoBehaviour, IProjectile
{
    private GameObject target;
    private Rigidbody rb;
    private float damage;
    [SerializeField] private float speed = 10f;

    [Header("Explosion")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private int ignoreArmor = 10;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private LayerMask enemyLayer; // QUAN TRỌNG: Gán layer của địch

    [Header("Rotation")]
    [Tooltip("Độ xoay bù trừ cho model (ví dụ: (90, 0, 0))")]
    [SerializeField] private Vector3 rotationFix = new Vector3(0, 0, 0);
    public AudioClip explosionSound;
    private Quaternion fixQuaternion;
    private IHealthSystem health;
    private bool hasExploded = false;
    private Collider myCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
        if (rb == null) Debug.LogError("Rigidbody component is missing from the projectile.");
        fixQuaternion = Quaternion.Euler(rotationFix);
    }

    void OnEnable()
    {
        hasExploded = false;
        myCollider.enabled = true;
        rb.isKinematic = false;
        target = null;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        if (hasExploded) return;

        if (target == null || !target.activeInHierarchy ||health.HasDie())
        {
            // Nếu mất mục tiêu, bay thẳng
            rb.linearVelocity = transform.forward * speed;
            ObjectPoolManager.ReturnObject(gameObject);
            return;
        }

        // --- LOGIC BAY VÀ XOAY (ĐÃ SỬA) ---
        Vector3 dir = (target.transform.position - rb.position).normalized;
        rb.linearVelocity = dir * speed;
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        rb.MoveRotation(targetRotation * fixQuaternion);
        // --- KẾT THÚC SỬA ---
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        // Nổ khi chạm mục tiêu HOẶC bất cứ thứ gì trên layer địch
        bool hitTarget = (other.gameObject == target);
        bool hitEnemyLayer = (enemyLayer.value & (1 << other.gameObject.layer)) != 0;

        if (hitTarget || hitEnemyLayer)
        {
            Explode();
        }
        // (Bạn cũng có thể thêm logic nổ khi chạm "Tường" (Wall layer) ở đây)
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        ObjectPoolManager.PlayAudio(explosionSound, transform.position, 1.0f);

        // Tắt mũi tên
        myCollider.enabled = false;
        rb.isKinematic = true;

        // Tạo hiệu ứng
        if (explosionEffect != null)
        {
            ObjectPoolManager.SpawnObject(explosionEffect, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Particle);
        }

        // --- TỐI ƯU: Chỉ quét layer địch ---
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);

        foreach (Collider collider in colliders)
        {
            IHealthSystem enemyHealth = collider.GetComponentInParent<IHealthSystem>();
            if (enemyHealth != null && !enemyHealth.HasDie())
            {
                if (enemyHealth is EnemyHealth eh) // Dùng 'is'
                {
                    eh.TakeDamage(damage, ignoreArmor);
                }
                else
                {
                    enemyHealth.TakeDamage(damage);
                }
            }
        }

        // Trả về pool (không cần coroutine vì đã tắt vật lý)
        ObjectPoolManager.ReturnObject(gameObject);
    }

    public void Launch(List<GameObject> targets, float damage)
    {
        // --- TỐI ƯU HÓA: TÌM MỤC TIÊU (O(n)) ---
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
        this.health = target.GetComponentInParent<IHealthSystem>();
        // --- KẾT THÚC TỐI ƯU ---

        this.damage = damage;
        hasExploded = false; // Đảm bảo bắt đầu chưa nổ

        if (this.target == null)
        {
            StartCoroutine(ReturnAfterDelay(5f));
        }
    }

    private IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPoolManager.ReturnObject(gameObject);
    }
}