using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class FireBall : MonoBehaviour, IProjectile
{
    [SerializeField] private float speed;

    Rigidbody rb;
    GameObject target;
    float damage;
    Collider hitCollider;
    [Tooltip("Tốc độ xoay ngẫu nhiên (độ/giây)")]
    [SerializeField] private float rotationSpeed = 360f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }



    // Update is called once per frame
    void Update()
    {
       
        if (target != null && target.activeInHierarchy)
        {
           
           Util.MoveToward(rb, target.transform, speed);
        }
        else
        {
          
            ObjectPoolManager.ReturnObject(gameObject);
        }
    }

    public void Launch(List<GameObject> target, float damage)
    {
        this.target = target.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault();

        hitCollider = this.target.GetComponentInChildren<Collider>();
        this.damage = damage;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other == hitCollider)
        {
            IHealthSystem heath = other.GetComponentInParent<IHealthSystem>();
            if (heath != null)
            {
                heath.TakeDamage(damage);
                ObjectPoolManager.ReturnObject(gameObject);
            }
            else
            {
                Debug.LogError("Target does not have IHealthSystem component.");
            }

        }
    }
}
