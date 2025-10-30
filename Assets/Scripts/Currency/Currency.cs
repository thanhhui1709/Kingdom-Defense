using TMPro;
using UnityEngine;
using DG.Tweening; // Import thư viện DOTween

public class Currency : MonoBehaviour
{
    // CƠ CHẾ SINGLETON ĐÃ SỬA CHỮA:
    public static Currency Instance;

    public TextMeshProUGUI coinText;

    [Header("Cài đặt Animation")]
    [Tooltip("Thời gian hiệu ứng đếm số tiền.")]
    public float countDuration = 0.5f;
    [Tooltip("Độ phóng to khi tiền thay đổi.")]
    public float punchScale = 1.2f;

    private int balance = 100; // Khởi tạo số dư ban đầu cho dễ test
    private int displayedBalance; // Số dư đang hiển thị trên UI

    private void Awake()
    {
        // Logic Singleton (đảm bảo chỉ có một instance)
        if (Instance == null)
        {
            Instance = this;
            // Khởi tạo số dư hiển thị bằng số dư thực tế
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
        // Đảm bảo UI hiển thị đúng ngay khi bắt đầu
        UpdateUI(0);

        // Đăng ký sự kiện (Giữ nguyên)
        if (GameEvent.Instance != null)
        {
            GameEvent.Instance.SubscribeEnemyDie(AddMoney);
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SubMoney(10);
        }  
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            AddMoney(100);
        }
    }



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
            Debug.Log("Sub" + amount + "Balance" + balance);
            UpdateUI(amount); 
            return true;
        }
        return false;
    }

   

    // --- HÀM MỚI: CẬP NHẬT UI CÓ ANIMATION ---
    /// <summary>
    /// Cập nhật UI với hiệu ứng đếm số (Counting) và hiệu ứng phóng to.
    /// </summary>
    /// <param name="changeAmount">Số tiền thay đổi (cho mục đích debug/effect).</param>
    /// <param name="animate">Có chạy animation phóng to UI không.</param>
    private void UpdateUI(int changeAmount)
    {
        // 1. Dừng mọi DOTween đang chạy trên text để tránh lỗi chồng chéo
        coinText.DOKill();


        // Tween số tiền đang hiển thị từ displayedBalance lên balance thực tế
        DOTween.To(() => displayedBalance, // Giá trị bắt đầu
                   x =>
                   {
                       displayedBalance = x; // Cập nhật displayedBalance
                       coinText.text = displayedBalance.ToString(); // Cập nhật Text
                   },
                   balance,              // Giá trị kết thúc (số dư thực tế)
                   countDuration);         // Thời gian chạy

    
      
    }
}