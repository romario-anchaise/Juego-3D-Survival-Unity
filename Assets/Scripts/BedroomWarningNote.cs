using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[DefaultExecutionOrder(-100)]
public sealed class BedroomWarningNote : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] GameObject prompt;
    [SerializeField] GameObject page;
    [SerializeField, Min(.1f)] float reach = 1.65f;
    [SerializeField] Collider aimTarget;
    [SerializeField] AudioSource porchAudio;
    [SerializeField] AudioClip firstStep;
    [SerializeField] AudioClip secondStep;
    bool footstepsTriggered;
    [SerializeField] UnityEngine.Events.UnityEvent onFirstClose = new UnityEngine.Events.UnityEvent();
    bool firstCloseTriggered;
    static int lastInteractionFrame = -1;
    public UnityEngine.Events.UnityEvent FirstReadClosed => onFirstClose;
    public static bool InputConsumedThisFrame => lastInteractionFrame == Time.frameCount;

    public bool CanInteract() => InteractionAim.IsTargeted(aimTarget, player, reach);

    void Update()
    {
        if (player == null || prompt == null || page == null)
            return;

        Vector3 eye = player.position + Vector3.up * 1.55f;
        Vector3 paper = transform.position + Vector3.up * .04f;
        Vector3 towardPaper = paper - eye;
        bool nearby = towardPaper.sqrMagnitude <= reach * reach
            && !Physics.Linecast(eye, paper, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        bool aimed = nearby && Cursor.lockState == CursorLockMode.Locked && CanInteract();

        var keyboard = Keyboard.current;
        if (page.activeSelf)
        {
            if (!nearby || (keyboard != null &&
                (keyboard.eKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)))
                ClosePage();
        }
        else if (aimed && keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            lastInteractionFrame = Time.frameCount;
            page.SetActive(true);
        }

        bool showPrompt = aimed && !page.activeSelf;
        if (prompt.activeSelf != showPrompt)
            prompt.SetActive(showPrompt);
    }

    void ClosePage()
    {
        page.SetActive(false);
        lastInteractionFrame = Time.frameCount;
        if (!firstCloseTriggered)
        {
            firstCloseTriggered = true;
            onFirstClose?.Invoke();
        }
        if (!footstepsTriggered && porchAudio != null && firstStep != null && secondStep != null)
        {
            footstepsTriggered = true;
            StartCoroutine(PorchFootsteps());
        }
    }

    IEnumerator PorchFootsteps()
    {
        yield return new WaitForSecondsRealtime(.55f);
        porchAudio.PlayOneShot(firstStep);
        yield return new WaitForSecondsRealtime(.9f);
        porchAudio.PlayOneShot(secondStep);
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (prompt != null)
            prompt.SetActive(false);
        if (page != null)
            page.SetActive(false);
    }
}
