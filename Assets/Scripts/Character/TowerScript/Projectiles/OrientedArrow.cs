using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

public class OrientedArrow : MonoBehaviour,IProjectile
{
    private GameObject target;
    private Rigidbody rb;
    private float damage;
    [SerializeField] private float speed = 10f;
    public GameObject explosionEffect;
    void Awake()
    {
        rb=GetComponent<Rigidbody>();
        if(rb==null) Debug.LogError("Rigidbody component is missing from the projectile.");

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target == null) return;

       Vector3 dir= Util.MoveToward(rb, target.transform, speed);
       Quaternion targetRotation= Quaternion.LookRotation(dir*180, transform.up)*Quaternion.Euler(90,0,0);
       rb.MoveRotation(targetRotation);
    }
  
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == target)
        {
            Destroy(gameObject);
            if(explosionEffect != null)
            {

                ObjectPoolManager.SpawnObject(explosionEffect, transform.position, Quaternion.identity,ObjectPoolManager.PoolType.Particle);
            }
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if(enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }
    }

    public void Launch(List<GameObject> target, float damage)
    {
        this.target = target.OrderBy(x=> Vector3.Distance(x.transform.position,transform.position)).FirstOrDefault();
        this.damage = damage;
    }
}
