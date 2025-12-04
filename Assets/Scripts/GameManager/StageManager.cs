using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Cần cho Image

public class StageManager : MonoBehaviour
{
    public List<SpawnEnemyWave> waves; // Kéo các spawner vào đây

    [Header("Win Condition")]
    [Tooltip("Tag (nhãn) của tất cả các GameObject kẻ địch")]
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Progress Bar UI")]
    [SerializeField] private Image progressBarIcon;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private GameObject progressBarContainer;

    // --- BIẾN ĐẾM (Đã sửa) ---
    private int totalWavesInLevel = 0; // Tổng số wave (ví dụ: 10)
    private int completedWaves = 0;  // Số wave đã hoàn thành (ví dụ: 3/10)
    private int activeEnemyCount = 0;
    private bool hasWon = false;

    void Start()
    {
        if (waves == null || waves.Count == 0) return;

        // --- SỬA LẠI START ---
        totalWavesInLevel = 0;
        completedWaves = 0;
        activeEnemyCount = 0;
        hasWon = false;

        // 1. Đăng ký sự kiện chết (Giữ nguyên)
        // (Bạn cần thêm hàm UnSubscribeEnemyDie vào GameEvent)
        GameEvent.Instance.SubscribeEnemyDie(OnEnemyDied);

        // 2. ĐẾM TỔNG SỐ WAVE
        foreach (var waveSpawner in waves)
        {
            // Hỏi spawner xem nó có bao nhiêu wave
            totalWavesInLevel += waveSpawner.GetTotalWaveCount();
            waveSpawner.SetManager(this);
        }
        // --- KẾT THÚC SỬA START ---

        UpdateProgressBar(); // Cập nhật UI lần đầu
    }

    private void OnDestroy()
    {
        // Nhớ hủy đăng ký
        if (GameEvent.Instance != null)
        {
            // (Bạn cần thêm hàm UnSubscribeEnemyDie vào GameEvent)
            // GameEvent.Instance.UnSubscribeEnemyDie(OnEnemyDied); 
        }
    }

    /// <summary>
    /// Được gọi bởi SpawnEnemyWave khi nó spawn 1 con
    /// </summary>
    public void ReportEnemySpawned()
    {
        activeEnemyCount++;
    }

    /// <summary>
    /// Được gọi bởi GameEvent khi 1 con quái chết
    /// </summary>
    private void OnEnemyDied(int money)
    {
        if (hasWon) return;
        activeEnemyCount--;
        if (activeEnemyCount < 0) activeEnemyCount = 0;

        CheckWinConditions(); // Kiểm tra lại khi quái chết
    }

    /// <summary>
    /// Được gọi bởi SpawnEnemyWave khi 1 EnemyWave hoàn thành
    /// </summary>
    public void ReportOneWaveCompleted() // Đổi tên
    {
        if (hasWon) return;

        completedWaves++;         // Đếm
        UpdateProgressBar();      // Cập nhật UI
        CheckWinConditions();   // Kiểm tra
    }

    private void UpdateProgressBar()
    {
        if (progressBarIcon == null || startPoint == null || endPoint == null)
            return;

        float progress = 0f;
        if (totalWavesInLevel > 0) // Tránh chia cho 0
        {
            // Bây giờ nó sẽ là 1/10, 2/10, 3/10...
            progress = (float)completedWaves / (float)totalWavesInLevel;
        }

        progressBarIcon.transform.position = Vector3.Lerp(
            startPoint.position,
            endPoint.position,
            progress
        );
    }

    /// <summary>
    /// Kiểm tra thắng (chỉ được gọi khi có sự kiện)
    /// </summary>
    private void CheckWinConditions()
    {
        if (hasWon) return;

        // Điều kiện: Đã hoàn thành TẤT CẢ wave VÀ không còn quái
        if (completedWaves == totalWavesInLevel && activeEnemyCount == 0)
        {
            hasWon = true;
            Debug.Log("YOU WIN! (Event-Driven)");

            if (progressBarContainer != null)
                progressBarContainer.SetActive(false);

            StartCoroutine(WinAfterTime(3f)); // Delay 1 giây trước khi win
        }
    }
    IEnumerator WinAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Debug.Log("YOU WIN! (After Time)");
        GameEvent.Instance.OnTriggerWinStage(); // (Hoặc WinGame)
    }
}