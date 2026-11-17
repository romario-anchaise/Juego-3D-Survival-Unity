using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

// Allows releasing the pointer in Play without fighting the movement controller.
[RequireComponent(typeof(StarterAssetsInputs))]
public class SurvivorCursor : MonoBehaviour
{
    StarterAssetsInputs input;
    float allowLockAfter;
    void Awake()
    {
        input = GetComponent<StarterAssetsInputs>();
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.stickyCursorLock = false;
        input.cursorLocked = false;
#endif
    }
    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SetLocked(false);
#else
        SetLocked(true);
#endif
    }
    void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Escape releases the browser pointer even when it also closes a note.
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            allowLockAfter = Time.unscaledTime + 1f;
            SetLocked(false);
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame
            && Time.unscaledTime >= allowLockAfter) SetLocked(true);
#else
        // Closing a note already consumes Escape; keep control of the camera.
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame
            && !BedroomWarningNote.InputConsumedThisFrame) SetLocked(false);
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) SetLocked(true);
#endif
        if (Cursor.lockState != CursorLockMode.Locked) { input.move = Vector2.zero; input.look = Vector2.zero; input.jump = false; input.sprint = false; }
    }
    void SetLocked(bool value)
    {
        input.cursorInputForLook = value;
#if UNITY_WEBGL && !UNITY_EDITOR
        // Suppress Starter Assets' automatic relock on application focus.
        input.cursorLocked = false;
#else
        input.cursorLocked = value;
#endif
        var target = value ? CursorLockMode.Locked : CursorLockMode.None;
        if (Cursor.lockState != target) Cursor.lockState = target;
        Cursor.visible = !value;
    }
    void OnDisable() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
}
