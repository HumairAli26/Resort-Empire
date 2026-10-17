using UnityEngine;
using UnityEngine.UI;

public class SpriteChangerTime : MonoBehaviour
{
    [Header("References")]
    public GameClockController clock;   // Drag the object with GameClockController here
    public Image uiImage;               // The UI Image to change
    public Sprite daySprite;            // Shown from dayStartHour until nightStartHour
    public Sprite nightSprite;          // Shown from nightStartHour until dayStartHour

    [Header("Switch Times (24-hour)")]
    [Range(0f, 24f)] public float nightStartHour = 21f; // 9 PM
    [Range(0f, 24f)] public float dayStartHour = 6f;    // 6 AM

    private bool isNight;
    private bool initialized;

    void Update()
    {
        if (clock == null || uiImage == null || daySprite == null || nightSprite == null)
            return;

        bool shouldBeNight = IsNight(clock.TimeOfDay);

        // Only change the sprite when the state actually changes
        if (!initialized || shouldBeNight != isNight)
        {
            isNight = shouldBeNight;
            initialized = true;
            uiImage.sprite = isNight ? nightSprite : daySprite;
        }
    }

    bool IsNight(float time)
    {
        // Night wraps past midnight: e.g. 21:00 -> 24:00 and 0:00 -> 6:00
        if (nightStartHour > dayStartHour)
            return time >= nightStartHour || time < dayStartHour;

        // Fallback if someone sets night to start before day in the same day
        return time >= nightStartHour && time < dayStartHour;
    }
}