using UnityEngine;

public class Currency 
{
    private int balance;
    public Currency(int initialAmount = 0)
    {
        balance = initialAmount;
    }
    // add
    public  void AddMoney (int amount)
    {
        if(amount > 0)
        {
            balance += amount;
        }
    }

    public bool SubMoney(int amount)
    {
        if (amount > 0)
        {
            if (balance > amount) { 
                balance -= amount;
                Debug.Log("Sub" + amount+ "Balance" + balance);
                return true;
            } else
            {
                return false;
            }
        }
        return false;
    }

    public int GetBalance() { 
        return balance;
    }
}
