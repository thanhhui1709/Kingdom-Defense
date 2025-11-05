using System;
using UnityEngine;

public class GameEvent : MonoBehaviour
{
    public static GameEvent Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;

        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        GameManager.Instance.GameEvent = this;
    }


    private Action onWinStage;
    private Action onWinGame;

    private Action<int> onEnemyDie;

    private Action<GameObject> onTowerLevelUp;

    private Action onGameOver;

    public void SubscribeWinStage(Action callback)
    {
        onWinStage += callback;
    }
    public void OnTriggerWinStage()
    {
        onWinStage?.Invoke();
    }
    public void SubscribeWinGame(Action callback)
    {
        onWinGame += callback;
    }
    public void OnTriggerWinGame()
    {
        onWinGame?.Invoke();
    }
    public void SubscribeGameOver(Action callback)
    {
        onGameOver += callback;
    }
    public void OnTriggerGameOver()
    {
        onGameOver?.Invoke();
    }

    public void SubscribeTowerLevelUp(Action<GameObject> callback)
    {
        onTowerLevelUp += callback;
    }
    public void OnTriggerTowerLevelUp(GameObject tower)
    {
        onTowerLevelUp?.Invoke(tower);
    }

    public void SubscribeEnemyDie(Action<int> callback)
    {
        onEnemyDie += callback;
    }
    public void OnTriggerEnemyDie(int cost)
    {
        onEnemyDie?.Invoke(cost);
    }
    public void UnSubscribeEnemyDie(Action<int> callback )
    {
        onEnemyDie -= callback;
    }
}
