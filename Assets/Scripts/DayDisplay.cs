
using UnityEngine;
using TMPro;

public class DayDisplay : MonoBehaviour
{
    [Header("References")]
    public GameClockController gameClock;
    public TMP_Text dayText;

    private int lastDisplayedDay = 0;

    void Start()
    {
        UpdateDayText();
    }

    void Update()
    {
        if (gameClock == null || dayText == null)
            return;

        if (gameClock.Day != lastDisplayedDay)
        {
            UpdateDayText();
        }
    }

    void UpdateDayText()
    {
        lastDisplayedDay = gameClock.Day;
        dayText.text = "Day " + lastDisplayedDay;
    }
}