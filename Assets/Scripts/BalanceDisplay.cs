using UnityEngine;
using TMPro;

public class BalanceDisplay : MonoBehaviour
{
    public TMP_Text balanceText;

    void Start()
    {
        EconomyManager.Instance.OnBalanceChanged += UpdateText;
        UpdateText(EconomyManager.Instance.CurrentBalance);
    }

    void OnDestroy()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnBalanceChanged -= UpdateText;
    }

    void UpdateText(int newBalance)
    {
        balanceText.text = "$" + newBalance.ToString("N0");
    }
}