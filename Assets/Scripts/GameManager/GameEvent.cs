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
    }


    private Action<int> onEnemyDie;

    private Action<GameObject> onTowerLevelUp;

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
}
