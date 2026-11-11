using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class WorkshopToolPickup : MonoBehaviour
{
    public enum PickupKind { WorkshopTool, Flashlight }
    [SerializeField] PickupKind kind;
    [SerializeField] PlayerInventory inventory;
    [SerializeField] Transform interactionPoint;
    [SerializeField] Collider aimTarget;
    [SerializeField] GameObject prompt;
    [SerializeField] GameObject confirmation;
    [SerializeField] GameObject notePage;
    [SerializeField] GameObject notePrompt;
    [SerializeField] Renderer[] toolRenderers;
    [SerializeField, Min(.1f)] float reach = 1.9f;

    bool collected;

    public bool CanInteract()
    {
        if (collected || inventory == null
            || (kind == PickupKind.Flashlight ? inventory.HasFlashlight : inventory.HasWorkshopTool)
            || interactionPoint == null || Time.timeScale <= 0
            || (notePage != null && notePage.activeSelf))
            return false;

        return InteractionAim.IsTargeted(aimTarget, inventory.transform, reach);
    }

    void Update()
    {
        bool reading = (notePage != null && notePage.activeSelf)
            || (notePrompt != null && notePrompt.activeSelf);
        bool nearby = !reading && !BedroomWarningNote.InputConsumedThisFrame
            && Cursor.lockState == CursorLockMode.Locked && CanInteract();
        if (prompt != null && prompt.activeSelf != nearby)
            prompt.SetActive(nearby);

        if (nearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            TryCollectFromPlayer();
    }

    public bool TryCollectFromPlayer()
    {
        if (!CanInteract()) return false;
        bool added = kind == PickupKind.Flashlight
            ? inventory.TryCollectFlashlight() : inventory.TryCollectWorkshopTool();
        if (!added) return false;
        collected = true;
        if (prompt != null) prompt.SetActive(false);
        foreach (var visual in toolRenderers)
            if (visual != null) visual.enabled = false;
        if (confirmation != null) confirmation.SetActive(true);
        StartCoroutine(AfterPickup());
        return true;
    }

    IEnumerator AfterPickup()
    {
        yield return new WaitForSeconds(2.7f);
        if (confirmation != null) confirmation.SetActive(false);
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (prompt != null) prompt.SetActive(false);
        if (confirmation != null) confirmation.SetActive(false);
    }
}
