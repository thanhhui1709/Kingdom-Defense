using UnityEngine;
using UnityEngine.UI;


public class Stats: MonoBehaviour
{
    public Sprite sprite;
    [SerializeField] private float heath;
    [SerializeField] private float ammor;
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float attackRange;
    [SerializeField] private float triggerRange;
    [SerializeField] private int   money;

    public float Heath { get => heath; set => heath = value; }
    public float Ammor { get => ammor; set => ammor = value; }
    public float AttackDamage { get => attackDamage; set => attackDamage = value; }
    public float AttackSpeed { get => attackSpeed; set => attackSpeed = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public float AttackRange { get => attackRange; set => attackRange = value; }
    public float TriggerRange { get => triggerRange; set => triggerRange = value; }

    public int Money { get => money; set => money = value; }

}