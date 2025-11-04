using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{
    public Sprite sprite;
    [SerializeField] private float heath;
    [SerializeField] private float ammor;
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float attackRange;
    [SerializeField] private float triggerRange;
    [SerializeField] private int money; // Đây là chi phí xây dựng cơ bản

    [SerializeField] private List<AttackBehavior> attackList;

    public float Heath { get => heath; set => heath = value; }
    public float Ammor { get => ammor; set => ammor = value; }
    public float AttackDamage { get => attackDamage; set => attackDamage = value; }
    public float AttackSpeed { get => attackSpeed; set => attackSpeed = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public float AttackRange { get => attackRange; set => attackRange = value; }
    public float TriggerRange { get => triggerRange; set => triggerRange = value; }
    public int Money { get => money; set => money = value; }

    public List<AttackBehavior> AttackList { get => attackList; set => attackList = value; }

    public AttackBehavior GetAttack(int id)
    {
        if (attackList == null || attackList.Count == 0)
        {
            return null;
        }
       
        return attackList[id];
    }

    // --- THÊM MỚI ---
    [Header("Runtime Stats")]
    [Tooltip("Tổng số tiền đã đầu tư vào trụ này (bao gồm cả xây và nâng cấp)")]
    [SerializeField] // Thêm SerializeField để dễ debug trong Inspector
    private int totalInvestedMoney;
    public int TotalInvestedMoney { get => totalInvestedMoney; set => totalInvestedMoney = value; }
    // --- KẾT THÚC THÊM MỚI ---

    //private void OnDrawGizmos()
    //{
    //    Vector3 position = transform.position;
    //    position = new Vector3(position.x, position.y + 0.1f, position.z);
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(position, attackRange);
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(position, triggerRange);
    //}
}