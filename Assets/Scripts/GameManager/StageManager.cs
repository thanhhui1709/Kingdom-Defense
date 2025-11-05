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

    // --- THÊM MỚI: UI CHO PROGRESS BAR ---
    [Header("Progress Bar UI")]
    [Tooltip("Icon (Image) sẽ di chuyển")]
    [SerializeField] private Image progressBarIcon;
    [Tooltip("Transform của điểm đầu (bên trái) thanh ngang")]
    [SerializeField] private Transform startPoint;
    [Tooltip("Transform của điểm cuối (bên phải) thanh ngang")]
    [SerializeField] private Transform endPoint;
    [Tooltip("GameObject cha chứa toàn bộ thanh progress (để ẩn khi thắng)")]
    [SerializeField] private GameObject progressBarContainer;
    // --- KẾT THÚC THÊM MỚI ---

    void Start()
    {
        // Bắt đầu Coroutine để kiểm tra điều kiện thắng
        StartCoroutine(CheckWinConditions());

        InvokeRepeating(nameof(UpdateProgressBar), 2, 1f);
    }

    // --- THÊM MỚI: HÀM UPDATE ---
    void Update()
    {
      
    }

    /// <summary>
    /// Tính toán tiến độ (từ 0.0 đến 1.0)
    /// </summary>
    private float CalculateProgress()
    {
        if (waves == null || waves.Count == 0) return 0f;

        float completedWaves = 0;
        foreach (var wave in waves)
        {
            // Đếm số spawner đã hoàn thành
            if (wave.isDoneAllWaveSpawned)
            {
                completedWaves++;
            }
        }

        // Trả về tỷ lệ (ví dụ: 3 / 10 = 0.3)
        return completedWaves / (float)waves.Count;
    }

    /// <summary>
    /// Cập nhật vị trí của Icon dựa trên tiến độ
    /// </summary>
    private void UpdateProgressBar()
    {
        if (progressBarIcon == null || startPoint == null || endPoint == null)
            return; // Chưa gán UI, bỏ qua

        // 1. Lấy tiến độ (ví dụ: 0.3)
        float progress = CalculateProgress();

        // 2. Dùng Lerp để tìm vị trí mới
        // (Nó sẽ nội suy 30% quãng đường từ Start đến End)
        progressBarIcon.transform.position = Vector3.Lerp(
            startPoint.position,
            endPoint.position,
            progress
        );
    }

    // (Hàm CheckDoneSpawn giữ nguyên)
    private bool CheckDoneSpawn()
    {
        foreach (var wave in waves)
        {
            if (!wave.isDoneAllWaveSpawned)
            {
                return false;
            }
        }
        return true;
    }

    // (Hàm CheckWinConditions giữ nguyên, chỉ thêm logic ẩn UI)
    IEnumerator CheckWinConditions()
    {
        yield return new WaitForSeconds(3f);

        while (true)
        {
            bool allWavesSpawned = CheckDoneSpawn();
            bool noEnemiesLeft = (GameObject.FindGameObjectWithTag(enemyTag) == null);

            if (allWavesSpawned && noEnemiesLeft)
            {
                Debug.Log("YOU WIN! Đã spawn hết và dọn sạch địch!");

                // Ẩn thanh progress khi thắng
                if (progressBarContainer != null)
                    progressBarContainer.SetActive(false);
                GameEvent.Instance.OnTriggerWinGame();
                yield break;

            }

        }


    }
}