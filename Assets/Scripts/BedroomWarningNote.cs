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
    [SerializeField] bool openOnApproach;
    [SerializeField] Transform approachFurniture;
    [SerializeField] AudioSource porchAudio;
    [SerializeField] AudioClip firstStep;
    [SerializeField] AudioClip secondStep;
    bool footstepsTriggered;
    [SerializeField] UnityEngine.Events.UnityEvent onFirstClose = new UnityEngine.Events.UnityEvent();
    bool firstCloseTriggered;
    bool openedDuringApproach;
    static int lastInteractionFrame = -1;
    public UnityEngine.Events.UnityEvent FirstReadClosed => onFirstClose;
    public static bool InputConsumedThisFrame => lastInteractionFrame == Time.frameCount;

    public bool CanInteract() => InteractionAim.IsTargeted(aimTarget, player, reach);

    void Update()
    {
        var keyboard = Keyboard.current;
        bool readPressed = keyboard != null && keyboard.eKey.wasPressedThisFrame;
        bool closePressed = readPressed || (keyboard != null && keyboard.escapeKey.wasPressedThisFrame);
        UpdateReading(Cursor.lockState == CursorLockMode.Locked, readPressed, closePressed);
    }

    void UpdateReading(bool lookEnabled, bool readPressed, bool closePressed)
    {
        if (player == null || prompt == null || page == null)
            return;

        Vector3 eye = player.position + Vector3.up * 1.55f;
        Vector3 paper = transform.position + Vector3.up * .04f;
        Vector3 towardPaper = paper - eye;
        bool nearby = towardPaper.sqrMagnitude <= reach * reach
            && !Physics.Linecast(eye, paper, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        bool aimed = nearby && lookEnabled && CanInteract();
        var view = Camera.main;
        Vector3 screen = view != null ? view.WorldToViewportPoint(paper) : Vector3.zero;
        bool approach = openOnApproach && lookEnabled
            && towardPaper.sqrMagnitude <= reach * reach
            && screen.z > 0 && screen.x > .08f && screen.x < .92f && screen.y > .08f && screen.y < .92f
            && VisibleOnApproach(view.transform.position, paper);
        if (openOnApproach) nearby = approach;
        if (!nearby) openedDuringApproach = false;

        if (page.activeSelf)
        {
            if (!nearby || closePressed)
                ClosePage();
        }
        else if (!InputConsumedThisFrame &&
            ((aimed && readPressed) || (approach && !openedDuringApproach)))
        {
            openedDuringApproach = true;
            lastInteractionFrame = Time.frameCount;
            page.SetActive(true);
        }

        bool showPrompt = aimed && !page.activeSelf;
        if (prompt.activeSelf != showPrompt)
            prompt.SetActive(showPrompt);
    }

    bool VisibleOnApproach(Vector3 from, Vector3 paper)
    {
        Vector3 direction = paper - from;
        foreach (var hit in Physics.RaycastAll(from, direction.normalized, direction.magnitude,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            if (approachFurniture == null || !hit.transform.IsChildOf(approachFurniture))
                return false;
        return true;
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
