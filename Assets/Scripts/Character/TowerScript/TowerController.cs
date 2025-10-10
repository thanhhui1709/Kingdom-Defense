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
    [SerializeField] private bool isMultiTarget;
    private HashSet<GameObject> targetEnemy=new();
    private float attackCooldown;

    void Start()
    {
        stats = GetComponent<Stats>();
    }

    // Update is called once per frame
    void Update()
    {
        targetEnemy=Util.FindGameObjectInRange(targetEnemy,transform,stats.TriggerRange,"Enemy");
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
                if (isMultiTarget)
                {
                    towerSkill.DoAttack(transform, projectilePrefab, targetEnemy.ToList());
                }
                else
                {
                    towerSkill.DoAttack(transform, projectilePrefab, targetEnemy.First());
                }
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
    }
}
