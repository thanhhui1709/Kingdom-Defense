using UnityEngine;
using UnityEngine.UI;


public class Stats: MonoBehaviour
{
    [SerializeField] public float heath;
    [SerializeField] public float ammor;
    [SerializeField] public float attackDamage;
    [SerializeField] public float attackSpeed;
    [SerializeField] public float moveSpeed;
    [SerializeField] public float attackRange;
    [SerializeField] public float triggerRange;

    public float Heath { get => heath; set => heath = value; }
    public float Ammor { get => ammor; set => ammor = value; }
    public float AttackDamage { get => attackDamage; set => attackDamage = value; }
    public float AttackSpeed { get => attackSpeed; set => attackSpeed = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public float AttackRange { get => attackRange; set => attackRange = value; }
    public float TriggerRange { get => triggerRange; set => triggerRange = value; }

}