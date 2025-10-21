using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class TowerController : MonoBehaviour
{
    [SerializeField] private Stats stats;
    [SerializeField] private ATowerSkill towerSkill;
    [SerializeField] private GameObject projectilePrefab;
  
  
    private HashSet<GameObject> targetEnemy=new();
    private float attackCooldown;

    void Start()
    {
        stats = GetComponent<Stats>();
    }

    // Update is called once per frame
    void Update()
    {
        targetEnemy=Util.FindGameObjectInRange(targetEnemy,transform,stats.AttackRange,"Enemy");
        RemoveDisableTarget();
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
        //return if no target
        if (targetEnemy.Count==0) return;
        if (targetEnemy.Count>0 || targetEnemy.All(x=>x.activeInHierarchy))
        {
            if (attackCooldown <= 0)
            {
               
                    towerSkill.DoAttack(transform, projectilePrefab, targetEnemy.ToList());
                    attackCooldown = 1f / stats.AttackSpeed;
            }

        }
       
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.AttackRange);
    }
    private void RemoveDisableTarget()
    {
        if(targetEnemy.Count==0) return;
        targetEnemy.RemoveWhere(x => !x.activeInHierarchy || Vector3.Distance(transform.position,x.transform.position)>stats.AttackRange);
        Debug.Log("Remove enemy");
        Debug.Log("After Remove: " + targetEnemy.Count);
    }
}
