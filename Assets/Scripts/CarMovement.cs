using System.Collections;
using UnityEngine;

public class TrafficCar2D : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform startPoint;
    public Transform stopPoint;
    public Transform endPoint;

    [Header("Car GameObjects")]
    public GameObject[] carObjects;   // Complete car GameObjects / Prefabs

    [Header("Settings")]
    public float speed = 5f;
    public float stopDuration = 3f;
    public float respawnDelay = 2f;

    private int currentCarIndex = 0;

    void Start()
    {
        if (carObjects == null || carObjects.Length == 0)
        {
            Debug.LogWarning("No car GameObjects assigned! Please assign at least one car.");
            return;
        }

        StartCoroutine(CarRoutine());
    }

    IEnumerator CarRoutine()
    {
        while (true)
        {
            // Get the current car
            GameObject currentCar = carObjects[currentCarIndex];

            // Make sure all other cars are hidden
            HideAllCars();

            // Activate current car
            currentCar.SetActive(true);

            // Move current car to the starting point
            currentCar.transform.position = startPoint.position;

            // Move from Start to Stop
            yield return StartCoroutine(
                MoveToPosition(currentCar.transform, stopPoint.position)
            );

            // Stop at the stop point
            yield return new WaitForSeconds(stopDuration);

            // Move from Stop to End
            yield return StartCoroutine(
                MoveToPosition(currentCar.transform, endPoint.position)
            );

            // Hide the car after reaching the end
            currentCar.SetActive(false);

            // Move to the next car
            currentCarIndex++;

            // Loop back to the first car
            if (currentCarIndex >= carObjects.Length)
            {
                currentCarIndex = 0;
            }

            // Wait before spawning the next car
            yield return new WaitForSeconds(respawnDelay);
        }
    }

    IEnumerator MoveToPosition(Transform car, Vector2 targetPosition)
    {
        while (Vector2.Distance(car.position, targetPosition) > 0.05f)
        {
            car.position = Vector2.MoveTowards(
                car.position,
                targetPosition,
                speed * Time.deltaTime
            );

            yield return null;
        }

        // Snap exactly to the target
        car.position = targetPosition;
    }

    void HideAllCars()
    {
        foreach (GameObject car in carObjects)
        {
            if (car != null)
            {
                car.SetActive(false);
            }
        }
    }
}