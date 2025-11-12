using System;
using UnityEngine;

public class GameEvent : MonoBehaviour
{
    public static GameEvent Instance;

    private void Awake()
    {
        if (Instance == null)
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

    // --- Win Stage ---
    public void SubscribeWinStage(Action callback) { onWinStage += callback; }
    public void OnTriggerWinStage() { onWinStage?.Invoke(); }

    // --- SỬA LẠI HÀM NÀY ---
    public void UnsubscribeWinStage(Action callback)
    {
        onWinStage -= callback; // Dùng 'callback'
    }

    // --- Win Game ---
    public void SubscribeWinGame(Action callback) { onWinGame += callback; }
    public void OnTriggerWinGame() { onWinGame?.Invoke(); }
    // (Bạn cũng nên thêm UnsubscribeWinGame)

    // --- Game Over ---
    public void SubscribeGameOver(Action callback) { onGameOver += callback; }
    public void OnTriggerGameOver() { onGameOver?.Invoke(); }

    // --- SỬA LẠI HÀM NÀY ---
    public void UnsubscribeGameOver(Action callback)
    {
        onGameOver -= callback; // Dùng 'callback'
    }

    // --- Tower Level Up ---
    public void SubscribeTowerLevelUp(Action<GameObject> callback) { onTowerLevelUp += callback; }
    public void OnTriggerTowerLevelUp(GameObject tower) { onTowerLevelUp?.Invoke(tower); }
    // (Bạn cũng nên thêm UnsubscribeTowerLevelUp)

    // --- Enemy Die ---
    public void SubscribeEnemyDie(Action<int> callback) { onEnemyDie += callback; }
    public void OnTriggerEnemyDie(int cost) { onEnemyDie?.Invoke(cost); }
    public void UnSubscribeEnemyDie(Action<int> callback) { onEnemyDie -= callback; }
}