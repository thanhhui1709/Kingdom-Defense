using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    public Sprite sprite;
    // GIỮ NGUYÊN CÁC BIẾN SERIALIZE FIELD CŨ CỦA BẠN
    [SerializeField] private float heath;
    [SerializeField] private float ammor;
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float attackRange;
    [SerializeField] private float triggerRange;
    [SerializeField] private int money;

    [SerializeField] private List<AttackBehavior> attackList;

    // --- CÁC BIẾN BACKUP (Lưu giá trị gốc) ---
    private float _startHealth;
    private float _startDamage;
    private int _startMoney;
    private bool _isInitialized = false; // Cờ kiểm tra đã backup chưa

    // GIỮ NGUYÊN CÁC PROPERTY (Để các script khác không bị lỗi)
    public float Heath { get => heath; set => heath = value; }
    public float Ammor { get => ammor; set => ammor = value; }
    public float AttackDamage { get => attackDamage; set => attackDamage = value; }
    public float AttackSpeed { get => attackSpeed; set => attackSpeed = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public float AttackRange { get => attackRange; set => attackRange = value; }
    public float TriggerRange { get => triggerRange; set => triggerRange = value; }
    public int Money { get => money; set => money = value; }
    public int TotalInvestedMoney;

    public List<AttackBehavior> AttackList { get => attackList; set => attackList = value; }

    private void Awake()
    {
        // Lưu lại giá trị gốc được chỉnh trong Inspector
        // Chỉ chạy 1 lần duy nhất khi Object được tạo ra (trước khi Pool dùng lại)
        if (!_isInitialized)
        {
            _startHealth = heath;
            _startDamage = attackDamage;
            _startMoney = money;
            _isInitialized = true;
        }
    }

    // --- HÀM NẠP CHỈ SỐ MỚI (Dùng BuffData) ---
    public void Initialize(BuffData buffData)
    {
        // Đảm bảo đã backup trước khi tính toán
        if (!_isInitialized) Awake();

        // Reset về gốc rồi nhân với Buff
        heath = _startHealth * buffData.HealthMultiplier;
        attackDamage = _startDamage * buffData.DamageMultiplier;
        money = Mathf.CeilToInt(_startMoney * buffData.MoneyMultiplier);

        // Các chỉ số khác giữ nguyên (hoặc reset lại nếu chúng bị thay đổi trong game)
        // Ví dụ nếu có skill làm chậm tốc chạy vĩnh viễn thì cần backup cả moveSpeed
    }

    public AttackBehavior GetAttack(int id)
    {
        if (attackList == null || attackList.Count == 0) return null;
        return attackList[id];
    }

    // (Phần Gizmos giữ nguyên)
}