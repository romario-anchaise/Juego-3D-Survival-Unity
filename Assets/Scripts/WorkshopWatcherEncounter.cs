using System.Collections;
using UnityEngine;

public sealed class WorkshopWatcherEncounter : MonoBehaviour
{
    public enum Phase { Dormant, Flicker, WaitingForSight, Seen, Gone }
    public Phase CurrentPhase { get; private set; }

    [SerializeField] GameObject watcher;
    [SerializeField] Transform head;
    [SerializeField] Transform player;
    [SerializeField, Min(2f)] float disappearDistance = 6f;
    [SerializeField] Light benchLight;
    [SerializeField] OldBulbFlicker bulbFlicker;
    [SerializeField] AudioSource treesAudio;
    [SerializeField] AudioSource tensionAudio;
    [SerializeField] AudioClip rustle;
    [SerializeField] AudioClip tension;

    float brightness;
    bool restoreFlicker;
    float visibleTime;
    float awayTime;
    float waitingTime;

    public void Begin()
    {
        if (CurrentPhase != Phase.Dormant || watcher == null || head == null) return;
        CurrentPhase = Phase.Flicker;
        brightness = benchLight != null ? benchLight.intensity : 0;
        restoreFlicker = bulbFlicker != null && bulbFlicker.enabled;
        StartCoroutine(Reveal());
    }

    IEnumerator Reveal()
    {
        if (bulbFlicker != null) bulbFlicker.enabled = false;
        yield return new WaitForSeconds(.35f);
        if (benchLight != null) benchLight.intensity = brightness * .08f;
        yield return new WaitForSeconds(.18f);
        if (benchLight != null) benchLight.intensity = brightness * .6f;
        yield return new WaitForSeconds(.12f);
        if (benchLight != null) benchLight.intensity = brightness;
        watcher.SetActive(true);
        CurrentPhase = Phase.WaitingForSight;
        if (treesAudio != null && rustle != null) treesAudio.PlayOneShot(rustle);
    }

    public bool IsInView()
    {
        var view = Camera.main;
        if (view == null || head == null) return false;
        Vector3 screen = view.WorldToViewportPoint(head.position);
        return screen.z > 0 && screen.x > .04f && screen.x < .96f
            && screen.y > .04f && screen.y < .96f
            && !Physics.Linecast(view.transform.position, head.position,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    void Update()
    {
        if (CurrentPhase != Phase.WaitingForSight && CurrentPhase != Phase.Seen) return;
        if (Time.timeScale <= 0) return;
        if (player != null && watcher != null)
        {
            Vector3 distance = player.position - watcher.transform.position;
            distance.y = 0;
            if (distance.sqrMagnitude <= disappearDistance * disappearDistance)
            {
                Finish();
                return;
            }
        }
        waitingTime += Time.deltaTime;
        bool visible = IsInView();
        if (CurrentPhase == Phase.WaitingForSight)
        {
            visibleTime = visible ? visibleTime + Time.deltaTime : 0;
            if (visibleTime >= .15f)
            {
                CurrentPhase = Phase.Seen;
                if (tensionAudio != null && tension != null) tensionAudio.PlayOneShot(tension);
            }
            else if (waitingTime > 45f) Finish();
        }
        else
        {
            awayTime = visible ? 0 : awayTime + Time.deltaTime;
            if (awayTime >= .3f) Finish();
        }
    }

    void Finish()
    {
        watcher.SetActive(false);
        CurrentPhase = Phase.Gone;
        RestoreLight();
    }

    void RestoreLight()
    {
        if (benchLight != null) benchLight.intensity = brightness;
        if (bulbFlicker != null && restoreFlicker) bulbFlicker.enabled = true;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (watcher != null) watcher.SetActive(false);
        if (CurrentPhase != Phase.Dormant) RestoreLight();
        if (treesAudio != null) treesAudio.Stop();
        if (tensionAudio != null) tensionAudio.Stop();
    }
}
