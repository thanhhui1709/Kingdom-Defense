using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkeletonArrow : MonoBehaviour,IProjectile
{
    private GameObject target;
    Collider collider;
    private Rigidbody rb;
    private float damage;
    [SerializeField] private float speed = 10f;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("Rigidbody component is missing from the projectile.");

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target == null) return;

        Util.MoveToward(rb, target.transform, speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == collider)
        {
          
            IHealthSystem enemyHealth = other.GetComponentInParent<IHealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            ObjectPoolManager.ReturnObject(gameObject);

        }
    }

    public void Launch(List<GameObject> target, float damage)
    {
        this.target = target.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault();
        this.damage = damage;
        collider=this.target.GetComponentInChildren<Collider>();
    }
}
