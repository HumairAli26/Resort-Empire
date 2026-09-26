using UnityEngine;
using TMPro;

public class GameStatsManager : MonoBehaviour
{
    public static GameStatsManager Instance;

    [Header("Current Values")]
    public int janitors = 0;
    public int visitors = 0;
    public int rooms = 0;
    public int plants = 0;
    public int facilities = 0;

    [Header("UI Text")]
    public TMP_Text janitorsText;
    public TMP_Text visitorsText;
    public TMP_Text roomsText;
    public TMP_Text plantsText;
    public TMP_Text facilitiesText;

    [Header("Popularity")]
    public TMP_Text popularityText;

    public int visitorsForMaxPopularity = 50;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
        UpdatePopularity();
    }

    public void AddJanitor()
    {
        janitors++;
        UpdateUI();
    }

    public void RemoveJanitor()
    {
        if (janitors > 0)
            janitors--;

        UpdateUI();
    }

    public void AddVisitor()
    {
        visitors++;
        UpdateUI();
        UpdatePopularity();
    }

    public void RemoveVisitor()
    {
        if (visitors > 0)
            visitors--;

        UpdateUI();
        UpdatePopularity();
    }

    public void AddRoom()
    {
        rooms++;
        UpdateUI();
    }

    public void RemoveRoom()
    {
        if (rooms > 0)
            rooms--;

        UpdateUI();
    }

    public void AddPlant()
    {
        plants++;
        UpdateUI();
    }

    public void RemovePlant()
    {
        if (plants > 0)
            plants--;

        UpdateUI();
    }

    public void AddFacility()
    {
        facilities++;
        UpdateUI();
    }

    public void RemoveFacility()
    {
        if (facilities > 0)
            facilities--;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (janitorsText != null)
            janitorsText.text = janitors.ToString();

        if (visitorsText != null)
            visitorsText.text = visitors.ToString();

        if (roomsText != null)
            roomsText.text = rooms.ToString();

        if (plantsText != null)
            plantsText.text = plants.ToString();

        if (facilitiesText != null)
            facilitiesText.text = facilities.ToString();
    }

    private void UpdatePopularity()
    {
        float popularity = (float)visitors / visitorsForMaxPopularity * 100f;

        popularity = Mathf.Clamp(popularity, 0f, 100f);

        if (popularityText != null)
        {
            popularityText.text = $"{popularity:0.0}%";
        }
    }
}