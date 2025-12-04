using UnityEngine;

// Su dung de mua nang cap sau cac man choi
public class Star : MonoBehaviour
{
    public static Star Instance;
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
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space)) {
          AddMoney(10);
        }
        
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
