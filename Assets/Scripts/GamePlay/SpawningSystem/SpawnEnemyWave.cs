using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SpawnEnemyWave : MonoBehaviour
{
    [Header("Wave")]
    [SerializeField] private float activeTime = 7f; // Thời gian chờ ban đầu
    [SerializeField] private List<EnemyWave> enemyWaves;

    [Header("Path")]
    [SerializeField] private List<PathNode> pathNodes;

    // --- Biến nội bộ ---
    private StageManager stageManager; // Tham chiếu đến Manager
    private int currentWaveIndex = 0;
    public bool isDoneAllWaveSpawned { get; private set; } = false;

    // Hàm này được gọi bởi StageManager lúc Start()
    public void SetManager(StageManager manager)
    {
        this.stageManager = manager;
    }

    // --- HÀM MỚI ---
    /// <summary>
    /// Báo cho StageManager biết spawner này có bao nhiêu wave
    /// </summary>
    public int GetTotalWaveCount()
    {
        return enemyWaves.Count;
    }

    private void Start()
    {
        // Bắt đầu chuỗi Coroutine chính
        StartCoroutine(SpawnAllWaves());
    }

    /// <summary>
    /// Coroutine chính: Quản lý việc spawn tất cả các wave
    /// </summary>
    private IEnumerator SpawnAllWaves()
    {
        yield return new WaitForSeconds(activeTime);

        for (currentWaveIndex = 0; currentWaveIndex < enemyWaves.Count; currentWaveIndex++)
        {
            EnemyWave currentWave = enemyWaves[currentWaveIndex];

            // 1. Chờ spawn xong 1 wave (ví dụ: wave 1/10)
            yield return StartCoroutine(SpawnWave(currentWave));

            // 2. Chờ thời gian nghỉ
            yield return new WaitForSeconds(currentWave.delayForTheNextWave);

            // 3. --- SỬA LỖI TẠI ĐÂY ---
            // BÁO CÁO CHO MANAGER: "Wave 1/10 đã xong!"
            // (Chúng ta đã di chuyển report vào BÊN TRONG vòng lặp)
            if (stageManager != null)
            {
                stageManager.ReportOneWaveCompleted();
            }
        }

        // 4. Đã xong TẤT CẢ
        isDoneAllWaveSpawned = true;
        // (Không cần báo cáo ở đây nữa, vì đã báo cáo từng cái)
    }

    /// <summary>
    /// Coroutine con: Spawn tất cả các WaveData trong 1 EnemyWave
    /// </summary>
    private IEnumerator SpawnWave(EnemyWave currentWave)
    {
        // Lặp qua các wave con (vd: "10 lính A", "5 lính B")
        foreach (var waveData in currentWave.waveData)
        {
            // Bắt đầu spawn wave con (vd: 10 lính A)
            yield return StartCoroutine(SpawnSingleWaveData(waveData));

            // Chờ (delay) trước khi bắt đầu wave con tiếp theo
            yield return new WaitForSeconds(waveData.delayForTheNextWaveData);
        }
    }

    /// <summary>
    /// Coroutine cháu: Spawn từng con quái
    /// </summary>
    private IEnumerator SpawnSingleWaveData(EnemyWave.WaveData wave)
    {
        for (int i = 0; i < wave.numberPerWave; i++)
        {
            // 1. Spawn quái
            var enemy = ObjectPoolManager.SpawnObject(wave.enemyPrefab, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Enemy);

            // 2. Gán đường đi (Path)
            MovementController enemyMovement = enemy.GetComponent<MovementController>();
            if (enemyMovement != null)
            {
                enemyMovement.SetPath(pathNodes);
            }

            // 3. BÁO CÁO CHO MANAGER: "1 con vừa ra lò!"
            if (stageManager != null)
            {
                stageManager.ReportEnemySpawned();
            }

            // 4. Chờ (delay) giữa các con quái
            yield return new WaitForSeconds(wave.delaySpawnPrefab);
        }
    }

    // --- CÁC HÀM CŨ (Load/Reset) ---
    public void Load(StageData stageData)
    {
        currentWaveIndex = stageData.index;
        if (currentWaveIndex < 0) currentWaveIndex = 0;
    }

    public void ResetWaveAfterClearStage()
    {
        currentWaveIndex = 0;
        isDoneAllWaveSpawned = false;
    }

    [System.Serializable]
    public struct StageData
    {
        public int index;
    }
}