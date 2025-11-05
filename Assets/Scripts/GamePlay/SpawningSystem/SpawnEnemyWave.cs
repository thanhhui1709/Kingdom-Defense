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

    // --- Biến "Báo cáo" ---
    // Được truy cập bởi StageManager
    public bool isDoneAllWaveSpawned { get; private set; } = false;

    // Hàm này được gọi bởi StageManager lúc Start()
    public void SetManager(StageManager manager)
    {
        this.stageManager = manager;
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
        // 1. Chờ thời gian active ban đầu
        yield return new WaitForSeconds(activeTime);

        // 2. Lặp qua từng EnemyWave (ví dụ: "Wave 1", "Wave 2")
        for (currentWaveIndex = 0; currentWaveIndex < enemyWaves.Count; currentWaveIndex++)
        {
            EnemyWave currentWave = enemyWaves[currentWaveIndex];

            // 3. Bắt đầu spawn các wave con (WaveData) bên trong EnemyWave
            yield return StartCoroutine(SpawnWave(currentWave));

            // 4. Chờ thời gian nghỉ (delay) trước khi bắt đầu Wave tiếp theo
            yield return new WaitForSeconds(currentWave.delayForTheNextWave);
        }

        // 5. ĐÃ HOÀN THÀNH
        // Đặt cờ báo hiệu đã xong
        isDoneAllWaveSpawned = true;

        // BÁO CÁO CHO MANAGER: "Tôi đã spawn xong!"
        if (stageManager != null)
        {
            stageManager.ReportWaveSpawnerCompleted();
        }
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
            // Chúng ta không 'yield' ở đây nếu không muốn chờ
            StartCoroutine(SpawnSingleWaveData(waveData));

            // Chờ (delay) trước khi bắt đầu wave con tiếp theo
            yield return new WaitForSeconds(waveData.delayForTheNextWaveData);
        }

        // (Không cần 'currentWave.isDoneSpawned' nữa,
        // vì chúng ta chỉ quan tâm khi 'isDoneAllWaveSpawned')
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
    // (Những hàm này có thể cần được xem lại,
    // vì chúng không còn phù hợp với logic Coroutine mới)

    public void Load(StageData stageData)
    {
        // Cảnh báo: Logic 'Load' phức tạp với Coroutine.
        // Cách đơn giản nhất là BẮT ĐẦU LẠI TỪ ĐẦU wave hiện tại.
        currentWaveIndex = stageData.index;
        if (currentWaveIndex < 0) currentWaveIndex = 0;

        // (Cần code phức tạp hơn để "resume" Coroutine)
    }

    public void ResetWaveAfterClearStage()
    {
        currentWaveIndex = 0;
        isDoneAllWaveSpawned = false;
    }

    [System.Serializable]
    public struct StageData
    {
        // (EnemyWave là class, có thể gây lỗi khi lưu/tải)
        // public EnemyWave currentWave; 
        public int index;
    }
}