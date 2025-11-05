using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Cần cho Image

public class StageManager : MonoBehaviour
{
    public List<SpawnEnemyWave> waves;

    [Header("Win Condition")]
    [Tooltip("Tag (nhãn) của tất cả các GameObject kẻ địch")]
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Progress Bar UI")]
    [SerializeField] private Image progressBarIcon;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private GameObject progressBarContainer;

    // --- BIẾN ĐẾM (MỚI) ---
    private int totalWaveSpawners = 0;
    private int completedWaveSpawners = 0;
    private int activeEnemyCount = 0;
    private bool hasWon = false; // Guard để tránh thắng 2 lần

    void Start()
    {
        if (waves == null || waves.Count == 0) return;

        // 1. Lấy tổng số spawner
        totalWaveSpawners = waves.Count;
        completedWaveSpawners = 0;
        activeEnemyCount = 0;
        hasWon = false;

        // 2. Đăng ký các sự kiện

        // a. Đăng ký vào sự kiện "EnemyDie" (bạn đã có sẵn)
        // (Giả sử EnemyHealth gọi GameEvent.Instance.OnTriggerEnemyDie)
        GameEvent.Instance.SubscribeEnemyDie(OnEnemyDied);

        // b. Yêu cầu từng Spawner báo cáo khi nó hoàn thành
        foreach (var waveSpawner in waves)
        {
            // Báo cho spawner biết "tôi" là ai (để nó gọi lại)
            waveSpawner.SetManager(this);
        }

        // 3. Cập nhật UI lần đầu
        UpdateProgressBar();
    }

    // Hủy đăng ký khi tắt
    private void OnDestroy()
    {
        GameEvent.Instance.UnSubscribeEnemyDie(OnEnemyDied); // (Bạn nên thêm hàm Unsubscribe)
    }

    // --- CÁC HÀM "BÁO CÁO" (MỚI) ---

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
    private void OnEnemyDied(int money) // (Hoặc 'OnEnemyDied()')
    {
        if (hasWon) return; // Đã thắng, không đếm nữa

        activeEnemyCount--;

        // An toàn: Đảm bảo số quái không bao giờ âm
        if (activeEnemyCount < 0) activeEnemyCount = 0;

        CheckWinConditions(); // Kiểm tra lại
    }

    /// <summary>
    /// Được gọi bởi SpawnEnemyWave khi nó hoàn thành
    /// </summary>
    public void ReportWaveSpawnerCompleted()
    {
        if (hasWon) return;

        completedWaveSpawners++;
        UpdateProgressBar(); // Cập nhật thanh progress
        CheckWinConditions(); // Kiểm tra lại
    }

    // --- LOGIC CŨ (ĐÃ SỬA) ---

    private void UpdateProgressBar()
    {
        if (progressBarIcon == null || startPoint == null || endPoint == null)
            return;

        // Tính toán (rất nhanh)
        float progress = (float)completedWaveSpawners / (float)totalWaveSpawners;

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
        // Đã thắng rồi, bỏ qua
        if (hasWon) return;

        // Điều kiện: Đã spawn hết VÀ không còn quái
        if (completedWaveSpawners == totalWaveSpawners && activeEnemyCount == 0)
        {
            hasWon = true;
            Debug.Log("YOU WIN! (Event-Driven)");

            if (progressBarContainer != null)
                progressBarContainer.SetActive(false);

            GameEvent.Instance.OnTriggerWinStage();
        }
    }
}