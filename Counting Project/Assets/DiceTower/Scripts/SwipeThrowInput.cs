using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/*
 * Turns a press-drag-release on any pointer (mouse, touch, pen) into a throw. The screen-space swipe is measured in
 * screen heights so it feels the same on every resolution, then mapped onto the camera's ground plane with an upward
 * lob. Strength comes from swipe speed. Space throws straight ahead at medium strength for desktop testing.
 * A press that starts on UI is ignored, so dragging across the dice panel never throws.
 */
public class SwipeThrowInput : MonoBehaviour
{
    [SerializeField] Camera viewCamera;
    [SerializeField, Tooltip("Shortest swipe that counts, in screen heights.")] float minSwipeLength = 0.06f;
    [SerializeField, Tooltip("Swipe speed in screen heights per second that gives full strength.")] float fullStrengthSpeed = 3f;
    [SerializeField] float maxSwipeDuration = 1.2f;
    [SerializeField, Tooltip("Upward component added to the throw direction.")] float lob = 0.45f;
    [SerializeField] Key keyboardThrow = Key.Space;

    public UnityEvent<Vector3, float> onSwipe = new UnityEvent<Vector3, float>();

    public bool InputEnabled { get; set; } = true;

    Vector2 pressPosition;
    float pressTime;
    bool tracking;

    void Awake()
    {
        if (viewCamera == null) viewCamera = Camera.main;
    }

    void Update()
    {
        if (!InputEnabled)
        {
            tracking = false;
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[keyboardThrow].wasPressedThisFrame)
        {
            onSwipe.Invoke(ToWorld(Vector2.up), 0.5f);
            return;
        }

        Pointer pointer = Pointer.current;
        if (pointer == null) return;

        if (pointer.press.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            tracking = true;
            pressPosition = pointer.position.ReadValue();
            pressTime = Time.unscaledTime;
        }
        else if (tracking && pointer.press.wasReleasedThisFrame)
        {
            tracking = false;
            Release(pointer.position.ReadValue(), Time.unscaledTime - pressTime);
        }
    }

    void Release(Vector2 releasePosition, float duration)
    {
        if (duration > maxSwipeDuration) return;

        Vector2 delta = (releasePosition - pressPosition) / Screen.height;
        float length = delta.magnitude;
        if (length < minSwipeLength) return;

        float speed = length / Mathf.Max(duration, 0.05f);
        float strength = Mathf.Clamp01(speed / fullStrengthSpeed);
        onSwipe.Invoke(ToWorld(delta / length), strength);
    }

    Vector3 ToWorld(Vector2 screenDirection)
    {
        Transform cam = viewCamera.transform;
        Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 ground = right * screenDirection.x + forward * screenDirection.y;
        return (ground.normalized + Vector3.up * lob).normalized;
    }
}
