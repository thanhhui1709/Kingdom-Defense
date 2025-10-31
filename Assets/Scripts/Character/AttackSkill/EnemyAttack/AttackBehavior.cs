using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackBehavior : ScriptableObject
{
    public abstract void Execute(MonoBehaviour runner,Stats attackerStats, GameObject target);
}
