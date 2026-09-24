using UnityEngine;
using System;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    [SerializeField] private int startingBalance = 10000;
    private int currentBalance;

    public int CurrentBalance => currentBalance;

    // Fires whenever balance changes, passing the new value
    public event Action<int> OnBalanceChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentBalance = startingBalance;
    }

    private void Start()
    {
        // Fire once at startup so UI initializes correctly
        OnBalanceChanged?.Invoke(currentBalance);
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return true; // nothing to spend
        if (currentBalance < amount)
        {
            Debug.Log("Not enough money!");
            return false;
        }

        currentBalance -= amount;
        OnBalanceChanged?.Invoke(currentBalance);
        return true;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        currentBalance += amount;
        OnBalanceChanged?.Invoke(currentBalance);
    }
}