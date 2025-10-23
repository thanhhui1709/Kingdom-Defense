using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PremiumPoisonShell : MonoBehaviour, IProjectile
{
    [SerializeField]
    private float speed=15f;

    private Transform targetPos;

    [SerializeField]
    private GameObject poisonSplash;

    private float damage;
    private Rigidbody rb;
    public void Launch(Transform launchPoint, List<GameObject> target, float damage)
    {
        targetPos = target.FirstOrDefault().transform;
        targetPos.position = new Vector3(targetPos.position.x, 0, targetPos.position.z);
        this.damage = damage;


    }
    private void OnEnable()
    {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb=GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Util.MoveToward(rb, targetPos, speed);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Road"))
        {
            if (poisonSplash != null)
            {
                GameObject splash = ObjectPoolManager.SpawnObject(poisonSplash, transform.position+new Vector3(0,-0.5f,0), Quaternion.identity, ObjectPoolManager.PoolType.TowerProjectile);
                PoisionSplash poisionSplash = poisonSplash.GetComponentInChildren<PoisionSplash>();
                if (poisonSplash != null)
                {
                    poisionSplash.damagePerTick = damage;

                }
            }
            ObjectPoolManager.ReturnObject(gameObject);
        }
    }
}
