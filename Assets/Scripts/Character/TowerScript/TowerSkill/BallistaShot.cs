using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Skill", menuName = "Tower Skill/BallistaShot")]
public class BallistaShot : ATowerSkill
{
    [SerializeField] private int numberOfEnemy;
    [SerializeField] private float launchOffset = 1.0f;

    public override void DoAttack(Transform shooter, GameObject projectile, List<GameObject> targets,float damage)
    {
        if (targets == null || targets.Count == 0) return;

        var sortedTargets = targets
            .Where(x => x != null && x.activeInHierarchy)
            .OrderBy(x => Vector3.Distance(shooter.position, x.transform.position))
            .Take(numberOfEnemy)
            .ToList();

        Vector3 spawnPosition = shooter.position + shooter.forward * launchOffset;

        foreach (var target in sortedTargets)
        {
            GameObject go = Instantiate(projectile, spawnPosition, Quaternion.identity);
            IProjectile projectileComp = go.GetComponent<IProjectile>();

            if (projectileComp == null)
            {
                Debug.LogError("Projectile does not implement IProjectile interface.");
                Destroy(go);
                continue;
            }

            projectileComp.Launch(shooter, new List<GameObject> { target }, damage);
        }
    }
}
