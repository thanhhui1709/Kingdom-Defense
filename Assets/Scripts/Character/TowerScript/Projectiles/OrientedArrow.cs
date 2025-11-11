using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class OrientedArrow : MonoBehaviour, IProjectile
{
    private GameObject target;
    private Rigidbody rb;
    private float damage;
    [SerializeField] private float speed = 10f;
    [SerializeField] private int ignoreArmor = 10;

    // --- SỬA LẠI BIẾN XOAY ---
    [Tooltip("Độ xoay bù trừ cho model (ví dụ: (90, 0, 0))")]
    [SerializeField] private Vector3 rotationFix = new Vector3(90, 0, 0);
    private Quaternion fixQuaternion;
    private IHealthSystem heath;
    // --- KẾT THÚC SỬA ---

    private bool isLaunched = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("Rigidbody component is missing from the projectile.");

        // Tính toán Quaternion 1 lần
        fixQuaternion = Quaternion.Euler(rotationFix);
    }

    void OnEnable()
    {
        isLaunched = false;
        target = null;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        // Chỉ bay khi đã được Launch và có mục tiêu
        if (!isLaunched || target == null || !target.activeInHierarchy||heath.HasDie())
        {
            // Nếu mất mục tiêu, bay thẳng
            rb.linearVelocity = transform.forward * speed;
            return;
        }

        // --- LOGIC BAY VÀ XOAY ĐÃ SỬA ---

        // 1. Tính hướng bay
        Vector3 dir = (target.transform.position - rb.position).normalized;

        // 2. Gán vận tốc
        rb.linearVelocity = dir * speed;

        // 3. Tính hướng xoay (nhìn theo hướng bay)
        Quaternion targetRotation = Quaternion.LookRotation(dir);

        // 4. Áp dụng bù trừ và xoay
        rb.MoveRotation(targetRotation * fixQuaternion);
        // --- KẾT THÚC SỬA ---
    }

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ kích hoạt 1 lần
        if (!isLaunched || !other.gameObject) return;

        if (other.gameObject == target)
        {
            IHealthSystem enemyHealth = other.GetComponentInParent<IHealthSystem>();
            if (enemyHealth != null && !enemyHealth.HasDie())
            {
                // Tối ưu: Dùng 'is' (C# 7.0+)
                if (enemyHealth is EnemyHealth eh)
                {
                    eh.TakeDamage(damage, ignoreArmor);
                }
                else
                {
                    enemyHealth.TakeDamage(damage);
                }
            }

            isLaunched = false; // Ngừng xử lý
            ObjectPoolManager.ReturnObject(gameObject);
        }
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
        heath = target.GetComponent<IHealthSystem>();
        // --- KẾT THÚC TỐI ƯU ---

        this.damage = damage;
        isLaunched = true;

        if (this.target == null)
        {
            // Nếu không có mục tiêu, tự hủy sau 5s
            StartCoroutine(ReturnAfterDelay(5f));
        }
    }

    private IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPoolManager.ReturnObject(gameObject);
    }
}