using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public List<SpawnEnemyWave> waves;

    [Header("Win Condition")]
    [Tooltip("Tag (nhãn) của tất cả các GameObject kẻ địch")]
    [SerializeField] private string enemyTag = "Enemy"; // <-- Đảm bảo kẻ địch của bạn có tag này

    void Start()
    {
        // Bắt đầu Coroutine để kiểm tra điều kiện thắng
        StartCoroutine(CheckWinConditions());
    }

    /// <summary>
    /// Kiểm tra xem tất cả các wave đã spawn xong chưa
    /// </summary>
    private bool CheckDoneSpawn()
    {
        foreach (var wave in waves)
        {
            // Chỉ cần 1 wave chưa xong -> return false
            if (!wave.isDoneAllWaveSpawned)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Coroutine chạy ngầm, kiểm tra điều kiện thắng mỗi giây
    /// </summary>
    IEnumerator CheckWinConditions()
    {
        // Chờ 2 giây lúc bắt đầu game
        yield return new WaitForSeconds(2f);

        while (true)
        {
           

     
            bool allWavesSpawned = CheckDoneSpawn();

        
            bool noEnemiesLeft = (GameObject.FindGameObjectWithTag(enemyTag) == null);

    
            if (allWavesSpawned && noEnemiesLeft)
            {
          
                Debug.Log("YOU WIN! Đã spawn hết và dọn sạch địch!");

                if (GameManager.Instance != null)
                {
                    GameEvent.Instance.OnTriggerWinStage();
                }
                yield break;
            }

            yield return new WaitForSeconds(3f);
        }
    }
}