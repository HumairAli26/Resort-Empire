using UnityEngine;
using UnityEngine.UI; // Required for interacting with the Slider component
using TMPro;         // Optional: Include if you want to use TextMeshPro for numbers
using UnityEngine.InputSystem;

public class ExpStats : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI levelText; // Optional text display
    [SerializeField] private TextMeshProUGUI expText;   // Optional text display

    [Header("Player Tracking Values")]
    public int currentLevel = 1;
    public float currentEXP = 0f;
    public float expToNextLevel = 100f; // Base starting EXP requirement

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        // Simulates earning 25 experience points whenever you press the Spacebar
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            GainExperience(25f);
        }
    }

    // Call this function from your quest, enemy death, or action scripts
    public void GainExperience(float amount)
    {
        currentEXP += amount;

        // Loop handles edge cases where an action gives enough EXP to level up multiple times
        while (currentEXP >= expToNextLevel)
        {
            currentEXP -= expToNextLevel; // Carry over excess EXP
            LevelUp();
        }

        UpdateUI();
    }

    private void LevelUp()
    {
        currentLevel++;
        
        // Scale formula: Increases difficulty by 20% each level
        expToNextLevel = Mathf.Round(expToNextLevel * 1.2f); 
        
        // Optional: Trigger level-up particle effects or audio here
        Debug.Log($"Leveled Up! You are now level {currentLevel}");
    }

    private void UpdateUI()
    {
        // 1. Calculate progress as a normalized decimal between 0f and 1f
        float fillPercentage = currentEXP / expToNextLevel;

        // 2. Assign value directly to Unity's slider fill amount
        if (expSlider != null)
        {
            expSlider.value = fillPercentage;
        }

        // 3. Optional: Update human-readable status text labels
        if (levelText != null) levelText.text = $"{currentLevel}";
        if (expText != null) expText.text = $"{currentEXP}";
    }
}
