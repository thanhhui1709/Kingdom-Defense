using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Skill", menuName = "Tower Skill/ContinuousLightningBeam")]
public class ContinuousLightningBeam : ATowerSkill
{
    // Chúng ta không cần biến gì ở đây vì logic sẽ nằm trong prefab
    HashSet<GameObject> hitTargets = new();
    // Giả sử hàm DoAttack gốc của bạn là đây

    public override void DoAttack(MonoBehaviour runner, Transform shooter, GameObject projectilePrefab, List<GameObject> targetsInRange, float damage)
    {
       
    }
}


