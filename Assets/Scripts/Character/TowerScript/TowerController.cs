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
    [SerializeField] private int maxTarget = 1;

    private enum TowerState
    {
        Idle,
        Attacking
    }

    private HashSet<GameObject> inRangeTarget = new();
    private TowerState currentState = TowerState.Idle;
    private HashSet<GameObject> currentTarget = new();
    private float attackCooldown;

    void Start()
    {
        stats = GetComponent<Stats>();
    }

    // Update is called once per frame
    void Update()
    {
        FindTarget();
        RemoveDisableTarget();
        //return if no target
        ExecuteState(Time.deltaTime);

    }
    private void FindTarget()
    {
        if (currentTarget.Count >= maxTarget) return;

        Util.FindGameObjectInRange(inRangeTarget, transform, stats.AttackRange, "Enemy");
     
        var bestTargets = inRangeTarget
            .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
            .Take(maxTarget);

        // Cập nhật lại danh sách mục tiêu chính bằng những mục tiêu tốt nhất vừa tìm được
        currentTarget = new HashSet<GameObject>(bestTargets);



    }

    private void ExecuteState(float deltaTime)
    {
        switch (currentState)
        {
            case TowerState.Idle:
                // Do nothing
                if (attackCooldown > 0)
                {
                    attackCooldown -= deltaTime;
                }
                if (currentTarget.Count > 0)
                {
                    currentState = TowerState.Attacking;
                }
                break;
            case TowerState.Attacking:

                if (currentTarget.Count > 0)
                {
                    if (attackCooldown <= 0)
                    {
                        towerSkill.DoAttack(transform, projectilePrefab, currentTarget.ToList());
                        attackCooldown = 1f / stats.AttackSpeed;
                    }
                    else
                    {
                        attackCooldown -= deltaTime;
                    }
                }
                else
                {
                    currentState = TowerState.Idle;
                }
                break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.AttackRange);
    }
    private void RemoveDisableTarget()
    {
        if (currentTarget.Count < maxTarget) return;
        currentTarget.RemoveWhere(x => !x.activeInHierarchy || Vector3.Distance(transform.position, x.transform.position) > stats.AttackRange);

    }
}
