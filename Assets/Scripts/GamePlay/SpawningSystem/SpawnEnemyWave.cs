using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpawnEnemyWave : MonoBehaviour
{
    [Header("Wave")]
    [SerializeField] private float activeTime = 7f;
    [SerializeField] private List<EnemyWave> enemyWaves;

    [Header("PathNode Enemy Move As Path")]
    [SerializeField] private List<PathNode> pathNodes;

    private EnemyWave currentWave;
    private int currentWaveIndex = 0;

    public bool isDoneAllWaveSpawned
    {
        get
        {
            return currentWaveIndex >= enemyWaves.Count;
        }
    }
    void Awake()
    {

        //GameManager.instance.StageManager = this;

    }
    private void Start()
    {
        //GameEvent.instance.TriggerStageStart();
        if ((currentWave == null || currentWaveIndex <= 0) && enemyWaves.Count > 0)
        {
            currentWaveIndex = 0;
            currentWave = enemyWaves[currentWaveIndex];
        }
        //if (enemyWaves.Count == 0)
        //{
        //    StartCoroutine(SpawnBoss());
        //}
        //else
        //{
            StartCoroutine(SpawnAllWaveAndBoss());
        //}

    }

    private void SetNextCurrentWave()
    {

        currentWaveIndex++;
        if (currentWaveIndex < enemyWaves.Count)
        {
            currentWave = enemyWaves[currentWaveIndex];

        }
    }
    // instantiate all enemyprefab from 1 wave data
    private IEnumerator SpawnSingleWaveData(EnemyWave.WaveData wave)
    {
        for (int i = 0; i < wave.numberPerWave; i++)
        {
            var enemy = ObjectPoolManager.SpawnObject(wave.enemyPrefab,transform.position,Quaternion.identity,ObjectPoolManager.PoolType.Enemy);

            // Pass the path nodes to the enemy's movement script
            MovementController enemyMovement = enemy.GetComponent<MovementController>();
            if (enemyMovement!=null)
            {
             enemyMovement.SetPath(pathNodes);
            }
            yield return new WaitForSeconds(wave.delaySpawnPrefab); // Delay between each enemy in wave
        }
        //StartCoroutine(SpawnDropItem(wave)); // Spawn drop items after all enemies are spawned
    }
    //private IEnumerator SpawnDropItem(EnemyWave.WaveData wave)
    //{
    //    if (wave.dropItems.Count == 0) yield break; // No items to drop
    //    yield return new WaitForSeconds(wave.droppedTime);
    //    foreach (var item in wave.dropItems)
    //    {
    //        GameObject dropItem = Instantiate(item, new Vector3(Random.Range(-15, 15), 15, 0), Quaternion.identity);

    //    }

    //}
    // instatiate all wave data(1 complete wave)
    private IEnumerator SpawnWave(EnemyWave currentWave)
    {
        foreach (var wave in currentWave.waveData)
        {
            if (wave.delayForTheNextWaveData > 0)
            {
                yield return StartCoroutine(SpawnSingleWaveData(wave));
                yield return new WaitForSeconds(wave.delayForTheNextWaveData);
            }
            else
            {
                StartCoroutine(SpawnSingleWaveData(wave));
            }
        }

        currentWave.isDoneSpawned = true;
    }

    // instantiate all enemy wave
    private IEnumerator SpawnAllWaveAndBoss()
    {
        yield return new WaitForSeconds(activeTime); // Wait for the active time before spawning the first wave
        while (currentWaveIndex < enemyWaves.Count)
        {
            yield return StartCoroutine(SpawnWave(currentWave)); // Wait until the wave is done
            //yield return new WaitUntil(() => CheckWaveClear(currentWave));
            SetNextCurrentWave();
            //GameManager.instance.SaveGame();
            yield return new WaitForSeconds(currentWave.delayForTheNextWave);
        }
        //if (CheckWaveClear(currentWave))
        //{
        //    StartCoroutine(SpawnBoss());

        //}
    }
    private bool CheckWaveClear(EnemyWave currentWave)
    {
        List<GameObject> enemy = GameObject.FindGameObjectsWithTag("Enemy").ToList();
        enemy.AddRange(GameObject.FindGameObjectsWithTag("Obstacle").ToList());
        return enemy.Count == 0 && currentWave.isDoneSpawned;
    }
    //private IEnumerator SpawnBoss()
    //{
    //    ThemeAudio.Instance.SetVolumeByTime(0, 2f);
    //    ThemeAudio.Instance.PlayThemeAudio(bossThemeAudio, 0.6f, 3);
    //    yield return new WaitForSeconds(spawnBossCountdown);
    //    GameObject boss = Instantiate(Boss, spawnPos, Quaternion.Euler(0, 0, 180));
    //    BossHealth bossHealth = boss.GetComponent<BossHealth>();
    //    if (bossHealth != null)
    //    {
    //        bossHealth.Initialize(bossHealthBar, bossHealthFill);
    //    }
    //}

    //public void Save(ref StageData stageData)
    //{
    //    stageData.currentWave = currentWave;
    //    stageData.index = currentWaveIndex;
    //}
    public void Load(StageData stageData)
    {
        currentWave = stageData.currentWave;
        currentWaveIndex = stageData.index;
        if (currentWaveIndex < 0)
        {
            currentWaveIndex = 0;

            currentWave = enemyWaves[currentWaveIndex];

        }

    }
    public void ResetWaveAfterClearStage()
    {
        currentWaveIndex = -1;
        currentWave = null;
    }


    [System.Serializable]
    public struct StageData
    {
        public EnemyWave currentWave;
        public int index;
    }
}