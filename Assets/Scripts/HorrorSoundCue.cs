using System.Collections;
using UnityEngine;

/// <summary>A localized, paced narrative sound. Musical cues can instead use a 2D source.</summary>
[DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
public sealed class HorrorSoundCue : MonoBehaviour
{
    public Transform player;
    public WorkshopWatcherEncounter encounter;
    public GameObject[] readingPages;
    public Transform triggerAnchor;
    public Transform lookTarget;
    public Vector3 lookOffset;
    [Min(0)] public float triggerRadius = 4;
    public bool requireLookAway;
    public bool oneShot = true;
    public bool foreground;
    [Min(0)] public float delay;
    [Min(0)] public float cooldown = 20;
    [Range(0, 1)] public float level = .3f;
    public Transform movingDoor;
    public float doorAngle = 10;
    public int TriggerCount { get; private set; }
    public bool IsOccluded { get; private set; }

    static float foregroundUntil;
    public static bool ForegroundActive => Time.time < foregroundUntil;
    AudioSource source;
    AudioLowPassFilter lowPass;
    Coroutine routine;
    float nextAllowed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSession() => foregroundUntil = 0;

    public static bool IsBusy(WorkshopWatcherEncounter watcher, GameObject[] pages)
    {
        if (Time.timeScale <= 0) return true;
        if (watcher != null && watcher.CurrentPhase != WorkshopWatcherEncounter.Phase.Dormant
            && watcher.CurrentPhase != WorkshopWatcherEncounter.Phase.Gone) return true;
        if (pages != null)
            foreach (var page in pages)
                if (page != null && page.activeInHierarchy) return true;
        return false;
    }

    void Awake()
    {
        source = GetComponent<AudioSource>();
        lowPass = GetComponent<AudioLowPassFilter>();
        source.playOnAwake = false;
    }

    void Update()
    {
        if (triggerAnchor == null || player == null || routine != null) return;
        if ((player.position - triggerAnchor.position).sqrMagnitude > triggerRadius * triggerRadius) return;
        if (lookTarget != null && IsVisible() == requireLookAway) return;
        Play();
    }

    bool IsVisible()
    {
        var camera = Camera.main;
        if (camera == null || lookTarget == null) return false;
        Vector3 target = lookTarget.TransformPoint(lookOffset);
        Vector3 screen = camera.WorldToViewportPoint(target);
        if (screen.z <= 0 || screen.x < .08f || screen.x > .92f
            || screen.y < .08f || screen.y > .92f) return false;
        return !Physics.Linecast(camera.transform.position, target, out var hit,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            || hit.transform.IsChildOf(lookTarget);
    }

    public void Play()
    {
        if (!isActiveAndEnabled || source == null || source.clip == null || routine != null
            || player == null || (oneShot && TriggerCount > 0) || Time.time < nextAllowed
            || IsBusy(encounter, readingPages)) return;
        if (source.spatialBlend > .5f
            && Vector3.Distance(player.position, transform.position) > source.maxDistance) return;
        routine = StartCoroutine(Perform());
    }

    IEnumerator Perform()
    {
        // Always yield once so even a zero-delay cue has a valid coroutine handle.
        yield return new WaitForSeconds(Mathf.Max(.01f, delay));
        if (foreground)
            while (Time.time < foregroundUntil) yield return null;
        if (IsBusy(encounter, readingPages)
            || (source.spatialBlend > .5f
                && Vector3.Distance(player.position, transform.position) > source.maxDistance))
        {
            routine = null;
            yield break;
        }
        TriggerCount++;
        nextAllowed = Time.time + cooldown;
        if (foreground) foregroundUntil = Time.time + source.clip.length + 10;
        float elapsed = 0;
        Quaternion initialDoor = movingDoor != null ? movingDoor.localRotation : Quaternion.identity;
        source.volume = 0;
        source.Play();
        while (elapsed < source.clip.length)
        {
            elapsed += Time.deltaTime;
            // Muffle sound behind solid walls; triggers and the player layer do not obstruct it.
            var camera = Camera.main;
            IsOccluded = source.spatialBlend > .5f && camera != null
                && Physics.Linecast(camera.transform.position, transform.position,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (lowPass != null)
                lowPass.cutoffFrequency = Mathf.Lerp(lowPass.cutoffFrequency,
                    IsOccluded ? 2400 : 19000, 8 * Time.deltaTime);
            float envelope = Mathf.Min(Mathf.Clamp01(elapsed / .05f),
                Mathf.Clamp01((source.clip.length - elapsed) / .18f));
            source.volume = level * envelope * (IsOccluded ? .65f : 1);
            if (movingDoor != null)
                movingDoor.localRotation = initialDoor * Quaternion.Euler(0,
                    doorAngle * Mathf.SmoothStep(0, 1, elapsed / source.clip.length), 0);
            yield return null;
        }
        source.Stop();
        routine = null;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (source != null) source.Stop();
        routine = null;
    }
}
