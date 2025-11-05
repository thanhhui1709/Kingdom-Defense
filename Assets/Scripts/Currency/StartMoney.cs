using UnityEngine;

// Su dung de mua nang cap sau cac man choi
public class StartMoney : MonoBehaviour
{
    public static StartMoney Instance;
    [SerializeField]
    private int startingMoney = 10;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public int GetBalance()
    {
        return startingMoney;
    }
    public void AddMoney(int amount)
    {
        startingMoney += amount;
    }
    public void RemoveMoney(int amount) 
    {
        startingMoney -= amount;
    }
    public bool CheckBalance(int amount)
    {
        return startingMoney >= amount;
    }
}
