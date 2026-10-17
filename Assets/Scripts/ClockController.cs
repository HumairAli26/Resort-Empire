using UnityEngine;

public class GameClockController : MonoBehaviour
{
    [Header("Clock Hands")]
    public Transform hourHand;
    public Transform minuteHand;

    [Header("Time Settings")]
    [Range(0f, 24f)] public float startHour = 9f;
    [Tooltip("How many real seconds one in-game hour takes")]
    public float realSecondsPerGameHour = 60f;
    public bool running = true;
    public bool smoothMovement = true;

    // Other systems (shops, day/night, etc.) can read these
    public float TimeOfDay { get; private set; }   // 0-24 in hours
    public int Day { get; private set; } = 1;
    public int Hour => Mathf.FloorToInt(TimeOfDay);
    public int Minute => Mathf.FloorToInt((TimeOfDay % 1f) * 60f);

    void Awake()
    {
        TimeOfDay = startHour;
    }

    void Update()
    {
        if (running)
            AdvanceTime(Time.deltaTime);

        UpdateHands();
    }

    void AdvanceTime(float realDeltaSeconds)
    {
        TimeOfDay += realDeltaSeconds / realSecondsPerGameHour;

        while (TimeOfDay >= 24f)
        {
            TimeOfDay -= 24f;
            Day++;
        }
    }

    void UpdateHands()
    {
        if (hourHand == null || minuteHand == null) return;

        float minuteAngle, hourAngle;

        if (smoothMovement)
        {
            minuteAngle = (TimeOfDay % 1f) * 360f;
            hourAngle   = (TimeOfDay % 12f) * 30f;
        }
        else
        {
            // Snap to whole in-game minutes
            minuteAngle = Minute * 6f;
            hourAngle   = ((Hour % 12) + Minute / 60f) * 30f;
        }

        // Negative because Unity's Z rotation is counter-clockwise
        minuteHand.localRotation = Quaternion.Euler(0f, 0f, -minuteAngle);
        hourHand.localRotation   = Quaternion.Euler(0f, 0f, -hourAngle);
    }

    // Call from other scripts, e.g. sleeping or skipping time
    public void SetTime(float hour)
    {
        TimeOfDay = Mathf.Repeat(hour, 24f);
    }
}