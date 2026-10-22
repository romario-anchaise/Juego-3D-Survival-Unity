using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

// Allows releasing the pointer in Play without fighting the movement controller.
[RequireComponent(typeof(StarterAssetsInputs))]
public class SurvivorCursor : MonoBehaviour
{
    StarterAssetsInputs input;
    void Awake() { input = GetComponent<StarterAssetsInputs>(); }
    void Start() { SetLocked(true); }
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) SetLocked(false);
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) SetLocked(true);
        if (Cursor.lockState != CursorLockMode.Locked) { input.move = Vector2.zero; input.look = Vector2.zero; input.jump = false; input.sprint = false; }
    }
    void SetLocked(bool value)
    {
        input.cursorLocked = value; input.cursorInputForLook = value;
        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !value;
    }
    void OnDisable() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
}
