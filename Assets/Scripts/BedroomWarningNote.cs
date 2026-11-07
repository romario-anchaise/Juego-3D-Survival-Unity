using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public sealed class BedroomWarningNote : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] GameObject prompt;
    [SerializeField] GameObject page;
    [SerializeField, Min(.1f)] float reach = 1.65f;
    [SerializeField] AudioSource porchAudio;
    [SerializeField] AudioClip firstStep;
    [SerializeField] AudioClip secondStep;
    bool footstepsTriggered;

    void Update()
    {
        if (player == null || prompt == null || page == null)
            return;

        Vector3 eye = player.position + Vector3.up * 1.55f;
        Vector3 paper = transform.position + Vector3.up * .04f;
        Vector3 towardPaper = paper - eye;
        bool nearby = towardPaper.sqrMagnitude <= reach * reach
            && !Physics.Linecast(eye, paper, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        var keyboard = Keyboard.current;
        if (page.activeSelf)
        {
            if (!nearby || (keyboard != null &&
                (keyboard.eKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)))
                ClosePage();
        }
        else if (nearby && keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            page.SetActive(true);
        }

        bool showPrompt = nearby && !page.activeSelf;
        if (prompt.activeSelf != showPrompt)
            prompt.SetActive(showPrompt);
    }

    void ClosePage()
    {
        page.SetActive(false);
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
