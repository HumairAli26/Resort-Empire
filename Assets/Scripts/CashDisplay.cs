using UnityEngine;
using TMPro;

public class CashDisplay : MonoBehaviour
{
    public TMP_Text cashText;

    void Start()
    {
         cashText.color = new Color32(35, 95, 45, 255);
        EconomyManager.Instance.OnBalanceChanged += UpdateCashText;
        UpdateCashText(EconomyManager.Instance.CurrentBalance);
    }

    void OnDisable()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnBalanceChanged -= UpdateCashText;
    }

    void UpdateCashText(int newAmount)
    {
        cashText.text = newAmount.ToString("N0");
    }
}