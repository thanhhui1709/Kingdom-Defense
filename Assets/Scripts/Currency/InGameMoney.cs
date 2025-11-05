using TMPro;
using UnityEngine;
using DG.Tweening; // Import thư viện DOTween

public class InGameMoney : MonoBehaviour
{
    public static InGameMoney Instance;
    public TextMeshProUGUI coinText;

    [Header("Cài đặt Animation")]
    public float countDuration = 0.5f;
    public float punchScale = 1.2f;

    private int balance = 100; // Khởi tạo số dư ban đầu
    private int displayedBalance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            displayedBalance = balance;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        UpdateUI(0); // Hiển thị số tiền ban đầu
        if (GameEvent.Instance != null)
        {
            GameEvent.Instance.SubscribeEnemyDie(AddMoney);
        }
    }

    // (Hàm Update test của bạn được giữ nguyên)
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) { SubMoney(10); }
        if (Input.GetKeyDown(KeyCode.Escape)) { AddMoney(100); }
    }

    // --- CÁC HÀM MỚI ---
    /// <summary>
    /// Lấy số dư hiện tại.
    /// </summary>
    public int GetBalance()
    {
        return balance;
    }

    /// <summary>
    /// Kiểm tra xem có đủ tiền hay không.
    /// </summary>
    public bool CheckBalance(int amount)
    {
        return balance >= amount;
    }
    // --- KẾT THÚC HÀM MỚI ---

    public void AddMoney(int amount)
    {
        if (amount > 0)
        {
            balance += amount;
            UpdateUI(amount);
        }
    }

    public bool SubMoney(int amount)
    {
        if (amount > 0 && balance >= amount)
        {
            balance -= amount;
            UpdateUI(amount); // (Bạn đã có sẵn UpdateUI, rất tốt)
            return true;
        }
        return false;
    }

    private void UpdateUI(int changeAmount)
    {
        coinText.DOKill();
        DOTween.To(() => displayedBalance,
                   x =>
                   {
                       displayedBalance = x;
                       coinText.text = displayedBalance.ToString();
                   },
                   balance,
                   countDuration);
    }
}