using System.Linq;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    [SerializeField] private Stats stats;
    [SerializeField] private ATowerSkill towerSkill;
    [SerializeField] private GameObject projectilePrefab;

    private GameObject targetEnemy;
    private float attackCooldown;
    void Start()
    {
        stats = GetComponent<Stats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
        if (targetEnemy == null || !targetEnemy.activeInHierarchy)
        {
            targetEnemy = Util.FindGameObjectInRange(transform, stats.TriggerRange, "Enemy").FirstOrDefault();
            Debug.Log("Find enemy: " + (targetEnemy != null ? targetEnemy.name : "null"));
        }
        else
        {
            if (attackCooldown <= 0)
            {
                towerSkill.DoAttack(transform, projectilePrefab, targetEnemy);
                attackCooldown = 1f / stats.AttackSpeed;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.TriggerRange);
    }
}
