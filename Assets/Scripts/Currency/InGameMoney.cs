using TMPro;
using UnityEngine;
using DG.Tweening; // Import thư viện DOTween

public class InGameMoney : MonoBehaviour
{
    public static InGameMoney Instance;
    public TextMeshProUGUI coinText;

    public int moneyPerSecond = 2;
    [Header("Cài đặt Animation")]
    public float countDuration = 0.5f;
    public float punchScale = 1.2f;

    [SerializeField]
    private int balance = 100; // Khởi tạo số dư ban đầu
    private int displayedBalance;
    private float timer = 0f;
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
     
        if (Input.GetKeyDown(KeyCode.Space)) { AddMoney(100); }
        timer += Time.deltaTime; 

        if (timer >= 1f) 
        {
            AddMoney(moneyPerSecond); 

            // Trừ đi 1 giây để bắt đầu đếm cho giây tiếp theo
            // (Dùng phép trừ tốt hơn gán = 0 để tránh sai số thời gian lâu dài)
            timer -= 1f;
        }

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