using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

// Giả định IProjectile và Util tồn tại
public class OrientedBom : MonoBehaviour, IProjectile
{
    private GameObject target;
    private Rigidbody rb;
    public LayerMask layer;
   
    private float damage ;

    [SerializeField] private float speed = 8f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private GameObject explosionEffect;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("Rigidbody component is missing from the bomb projectile.");
    }

    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 dir = Util.MoveToward(rb, target.transform, speed);
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
        rb.MoveRotation(targetRotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == target)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        Debug.Log("Bom nổ! Đã tìm thấy " + colliders.Length + " collider trong bán kính.");

        foreach (Collider nearby in colliders)
        {
            EnemyHealth health = nearby.GetComponent<EnemyHealth>();
            if (health != null)
            {
                // Sử dụng giá trị damage đã được truyền vào từ TowerController
                health.TakeDamage(damage);
                Debug.Log("SUCCESS: " + nearby.gameObject.name + " đã nhận sát thương: " + damage);
            }

            Rigidbody rbNearby = nearby.GetComponent<Rigidbody>();
            if (rbNearby != null)
            {
                rbNearby.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
        StartCoroutine(WaitToDestroy(1f));
        Debug.Log("Bomb đã hoàn thành chức năng và bị hủy!");

    }
    IEnumerator WaitToDestroy(float delay)
    {
        yield return new WaitForSeconds(delay); 
        ObjectPoolManager.ReturnObject(gameObject);
    
    }

    
    public void Launch( List<GameObject> targets, float damage)
    {
      
        this.target = targets.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault();

        
        this.damage = damage;

        Debug.Log("Bom đã được Launch với sát thương được thiết lập: " + this.damage);
    }
}