using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class TowerController : MonoBehaviour
{
    private Stats stats;
    [SerializeField] private ATowerSkill towerSkill;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int maxTarget = 1;
    [SerializeField] private GameObject shooter;

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
                    // Xoay nòng súng theo mục tiêu
                    GameObject target = currentTarget.FirstOrDefault();
                    RotateShooterTowardTarget(target.transform);

                    if (attackCooldown <= 0)
                    {
                        towerSkill.DoAttack(shooter.transform, projectilePrefab, currentTarget.ToList(), stats.AttackDamage);
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

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, stats.AttackRange);
    //}
    private void RemoveDisableTarget()
    {
        if (currentTarget.Count < maxTarget) return;
        currentTarget.RemoveWhere(x => !x.activeInHierarchy || Vector3.Distance(transform.position, x.transform.position) > stats.AttackRange);

    }
    public HashSet<GameObject> GetCurrentTargets()
    {
        return currentTarget;
    }

    private void RotateShooterTowardTarget(Transform target)
    {
        float rotationSpeed = 10f;
        // 1. Tính toán hướng cần nhìn
        Vector3 directionToTarget = target.position - shooter.transform.position;
        // Đặt Y = 0 nếu bạn chỉ muốn xoay ngang (trụ không ngước lên/cúi xuống)
        // Nếu muốn cả ngước lên/cúi xuống thì bỏ dòng này:
        directionToTarget.y = 0;

        // 2. Tạo Quaternion xoay mục tiêu
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        Quaternion rotationOffset = Quaternion.Euler(0, 180, 0);
        targetRotation *= rotationOffset;

        // 3. Xoay mượt mà bằng Quaternion.Slerp
        shooter.transform.rotation = Quaternion.Slerp(
            shooter.transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );

        // LƯU Ý QUAN TRỌNG: Bạn có thể cần điều chỉnh offset
        // Nếu mô hình nòng súng của bạn quay 90 độ so với hướng bay mong muốn, 
        // bạn có thể cần thêm một offset:
        // shooter.transform.rotation *= Quaternion.Euler(0, 90, 0); 
    }
}
