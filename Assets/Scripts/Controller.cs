using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class Controller : MonoBehaviour
{
    [SerializeField] private Vector2 panInput;
    [SerializeField] private float panSpeed = 30f;
    [SerializeField] private float zoomSpeed = 5f;

    [SerializeField] private Vector2 MinCameraBounds = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 MaxCameraBounds = new Vector2(10f, 10f);

    private float targetZoom;
    private float zoomSensitivity = 1f;
    [SerializeField] private Vector2 zoomLimits = new Vector2(5f, 15f);

    [SerializeField] private CinemachineCamera cinemachineCamera;

    private bool selectInitiated = false;

    void Awake()
    {
        targetZoom = cinemachineCamera.Lens.OrthographicSize;
    }

    void Update()
    {
        HandleCamera();
    }

    public void OnPan(InputAction.CallbackContext context) 
    {
        panInput = context.ReadValue<Vector2>();
    }

    public void OnZoom(InputAction.CallbackContext context) 
    {
        targetZoom = Mathf.Clamp(targetZoom + context.ReadValue<float>() * zoomSensitivity, 
            zoomLimits.x, zoomLimits.y);
    }

    private void HandleCamera()
    {
        Vector3 newPos = transform.position += new Vector3(panInput.x, panInput.y, transform.position.z) * Time.deltaTime * panSpeed;
        transform.position = new Vector3(
            Mathf.Clamp(newPos.x, MinCameraBounds.x, MaxCameraBounds.x),
            Mathf.Clamp(newPos.y, MinCameraBounds.y, MaxCameraBounds.y),
            newPos.z
        );

        cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(cinemachineCamera.Lens.OrthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
    }

    /*void HandleSelection()
    {
        if (selectInitiated)
        {
            if (!stopSelectionScanning)
            {
                HandleSelectionScanning();
            }
            if (currentSelectedObject && currentSelectedObject.TryGetComponent(out Building building))
            {
                stopSelectionScanning = true;
                building.SetSelected(true);
            }
        }
        else
        {
            if (currentSelectedObject && currentSelectedObject.TryGetComponent(out Building building))
            {
                building.SetSelected(false);
            }
            stopSelectionScanning = false;
            currentSelectedObject = null;
        }
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            selectInitiated = true;
        }
        else if (context.canceled)
        {
            selectInitiated = false;
        }
    }*/
}
