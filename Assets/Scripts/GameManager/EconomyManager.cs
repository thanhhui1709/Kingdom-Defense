using UnityEngine;

public class EconomyManager : MonoBehaviour
{

    private Currency playerMoney;
   
    void Start()
    {
        playerMoney = new Currency(100);
        playerMoney.AddMoney(50);   
        playerMoney.SubMoney(120); 
        Debug.Log("Số dư cuối: " + playerMoney.GetBalance());
    }

    
}
