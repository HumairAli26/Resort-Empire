using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MobileCameraPan : MonoBehaviour
{
    [Header("Pan Settings")]
    public float panSpeed = 0.5f;

    [Header("Camera Bounds")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Vector3 touchStart;

    void Update()
    {
        // ==========================================
        // 1. HANDLE MOBILE TOUCH / DRAG
        // ==========================================

        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.isPressed)
            {
                Vector2 touchPosition = touch.position.ReadValue();

                // Don't move camera if touching UI
                if (EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                // Touch just started
                if (touch.press.wasPressedThisFrame)
                {
                    touchStart = GetWorldPosition(touchPosition);
                }

                // Touch is moving
                if (touch.delta.ReadValue() != Vector2.zero)
                {
                    Vector3 direction =
                        touchStart - GetWorldPosition(touchPosition);

                    Vector3 targetPosition =
                        transform.position + direction;

                    transform.position =
                        ClampPosition(targetPosition);

                    // Update starting position so movement stays smooth
                    touchStart = GetWorldPosition(touchPosition);
                }
            }
        }


        // ==========================================
        // 2. HANDLE KEYBOARD ARROW KEYS
        // ==========================================

        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.isPressed)
            {
                MoveUp();
            }

            if (Keyboard.current.downArrowKey.isPressed)
            {
                MoveDown();
            }

            if (Keyboard.current.leftArrowKey.isPressed)
            {
                MoveLeft();
            }

            if (Keyboard.current.rightArrowKey.isPressed)
            {
                MoveRight();
            }


            // ==========================================
            // 3. HANDLE WASD
            // ==========================================

            if (Keyboard.current.wKey.isPressed)
            {
                MoveUp();
            }

            if (Keyboard.current.sKey.isPressed)
            {
                MoveDown();
            }

            if (Keyboard.current.aKey.isPressed)
            {
                MoveLeft();
            }

            if (Keyboard.current.dKey.isPressed)
            {
                MoveRight();
            }
        }
    }


    // ==========================================
    // 4. UI BUTTON METHODS
    // ==========================================

    public void MoveUp()
    {
        MoveCamera(Vector3.forward);
    }

    public void MoveDown()
    {
        MoveCamera(Vector3.back);
    }

    public void MoveLeft()
    {
        MoveCamera(Vector3.left);
    }

    public void MoveRight()
    {
        MoveCamera(Vector3.right);
    }


    // ==========================================
    // 5. MOVE CAMERA
    // ==========================================

    private void MoveCamera(Vector3 direction)
    {
        Vector3 targetPosition =
            transform.position +
            (direction * panSpeed * 20f * Time.deltaTime);

        transform.position =
            ClampPosition(targetPosition);
    }


    // ==========================================
    // 6. GET WORLD POSITION FROM TOUCH
    // ==========================================

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        if (Camera.main == null)
        {
            return Vector3.zero;
        }

        Ray ray =
            Camera.main.ScreenPointToRay(screenPosition);

        // Ground plane at Y = 0
        Plane groundPlane =
            new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }


    // ==========================================
    // 7. CAMERA BOUNDARIES
    // ==========================================

    private Vector3 ClampPosition(Vector3 targetPos)
    {
        // X = left/right
        targetPos.x = Mathf.Clamp(
            targetPos.x,
            minBounds.x,
            maxBounds.x
        );

        // Z = forward/back
        targetPos.z = Mathf.Clamp(
            targetPos.z,
            minBounds.y,
            maxBounds.y
        );

        return targetPos;
    }
}