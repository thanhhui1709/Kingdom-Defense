using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

public class OrientedBom : MonoBehaviour, IProjectile
{
    private GameObject target;
    private Rigidbody rb;

    [SerializeField] private float speed = 8f;        // tốc độ bay của bom
    [SerializeField] private float explosionRadius = 3f; // bán kính nổ
    [SerializeField] private float explosionForce = 500f; // lực nổ
    [SerializeField] private GameObject explosionEffect;  // prefab hiệu ứng nổ

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("Rigidbody component is missing from the bomb projectile.");
    }

    void FixedUpdate()
    {
        if (target == null) return;

        // Tính hướng di chuyển
        Vector3 dir = Util.MoveToward(rb, target.transform, speed);

        // Di chuyển bom về phía target
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);

        // Xoay bom hướng về target
        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
        rb.MoveRotation(targetRotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == target)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Hiệu ứng nổ
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Tìm tất cả object trong bán kính nổ
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearby in colliders)
        {
            Rigidbody rbNearby = nearby.GetComponent<Rigidbody>();
            if (rbNearby != null)
            {
                rbNearby.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        Debug.Log("Bomb exploded!");
        Destroy(gameObject);
    }

    public void Launch(Transform launchPoint, List<GameObject> targets)
    {
        // Chọn target gần nhất
        this.target = targets.OrderBy(x => Vector3.Distance(x.transform.position, launchPoint.position)).FirstOrDefault();
    }
}
