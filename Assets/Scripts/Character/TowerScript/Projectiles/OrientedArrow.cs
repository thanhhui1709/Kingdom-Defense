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
    public int ignoreArmor = 10;
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
            if (explosionEffect != null)
            {

                ObjectPoolManager.SpawnObject(explosionEffect, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Particle);
            }
            IHealthSystem enemyHealth = other.GetComponent<IHealthSystem>();
            if (enemyHealth != null)
            {
               if(enemyHealth as EnemyHealth)
                {
                    EnemyHealth eh = enemyHealth as EnemyHealth;
                    eh.TakeDamage(damage,10);
                }
                else
                {
                    enemyHealth.TakeDamage(damage);
                }
            }
            ObjectPoolManager.ReturnObject(gameObject);
           
        }
    }

    public void Launch(List<GameObject> target, float damage)
    {
        this.target = target.OrderBy(x=> Vector3.Distance(x.transform.position,transform.position)).FirstOrDefault();
        this.damage = damage;
    }
}
