using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>A localized, paced narrative sound. Musical cues can instead use a 2D source.</summary>
[DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
public sealed class HorrorSoundCue : MonoBehaviour
{
    public Transform player;
    public WorkshopWatcherEncounter encounter;
    public GameObject[] readingPages;
    public Transform triggerAnchor;
    public HorrorSoundCue playAfter;
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
    [Min(0)] public float doorTravelSeconds;
    public bool reopenableDoor;
    public GameObject windowFigure;
    public bool revealFigureOnAudioCut;
    public AudioClip[] escapeFootsteps;
    public Font escapeWarningFont;
    [Min(0)] public float figureDelay = 0.8f;
    [Min(0)] public float figureDuration = 0.65f;
    public int TriggerCount { get; private set; }
    public bool IsOccluded { get; private set; }

    static float foregroundUntil;
    public static bool ForegroundActive => Time.time < foregroundUntil;
    AudioSource source;
    AudioLowPassFilter lowPass;
    Coroutine routine;
    float nextAllowed;
    bool doorClosed;
    Quaternion openDoorRotation;
    bool reopenedByPlayer;
    bool escapeArmed;
    float warningUntil;
    Vector3 previousEscapePosition;
    GUIStyle escapeWarningStyle;

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
        UpdateEscape();
        if (CanReopenDoor() && Keyboard.current?.eKey.wasPressedThisFrame == true
            && !BedroomWarningNote.InputConsumedThisFrame)
        {
            doorClosed = false;
            reopenedByPlayer = true;
            routine = StartCoroutine(ReopenDoor());
            return;
        }
        if (triggerAnchor == null || player == null || routine != null) return;
        if (oneShot && TriggerCount > 0) return;
        if (playAfter != null && (playAfter.TriggerCount == 0 || playAfter.routine != null || playAfter.source.isPlaying)) return;
        if ((player.position - triggerAnchor.position).sqrMagnitude > triggerRadius * triggerRadius) return;
        if (lookTarget != null && IsVisible() == requireLookAway) return;
        Play();
    }

    bool CanReopenDoor()
    {
        if (!reopenableDoor || !doorClosed || routine != null || movingDoor == null || player == null
            || Cursor.lockState != CursorLockMode.Locked || IsBusy(encounter, readingPages))
            return false;

        var view = Camera.main;
        if (view == null) return false;
        Ray aim = view.ViewportPointToRay(new Vector3(.5f, .5f));
        return Physics.Raycast(aim, out var hit, Vector3.Distance(aim.origin, player.position) + 3f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            && hit.transform.IsChildOf(movingDoor)
            && (hit.point - (player.position + Vector3.up * 1.4f)).sqrMagnitude <= 2.4f * 2.4f;
    }

    void OnGUI()
    {
        if (Time.time < warningUntil && !IsBusy(encounter, readingPages))
            DrawEscapeWarning();
        if (!CanReopenDoor()) return;
        GUI.Box(new Rect(Screen.width * .5f - 80, Screen.height * .5f + 24, 160, 28),
            "[E] Abrir puerta");
    }

    void DrawEscapeWarning()
    {
        const string message = "¡TE ENCONTRÓ!";
        escapeWarningStyle ??= new GUIStyle(GUI.skin.label)
        {
            font = escapeWarningFont,
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        escapeWarningStyle.normal.textColor = new Color(1f, .65f, .57f);
        float width = Mathf.Clamp(escapeWarningStyle.CalcSize(new GUIContent(message)).x + 48, 238, 390);
        Color previousColor = GUI.color;
        GUI.color = new Color(.07f, .035f, .042f, .94f);
        GUI.DrawTexture(new Rect(24, 24, width, 48), Texture2D.whiteTexture);
        GUI.color = new Color(.78f, .15f, .16f);
        GUI.DrawTexture(new Rect(24, 24, 3, 48), Texture2D.whiteTexture);
        GUI.color = previousColor;
        GUI.Label(new Rect(42, 24, width - 28, 48), message, escapeWarningStyle);
    }

    IEnumerator TurnDoor(Quaternion destination)
    {
        Quaternion start = movingDoor.localRotation;
        float duration = Mathf.Max(.05f, doorTravelSeconds);
        for (float elapsed = 0; elapsed < duration; elapsed += Time.deltaTime)
        {
            movingDoor.localRotation = Quaternion.Slerp(start, destination,
                Mathf.SmoothStep(0, 1, elapsed / duration));
            yield return null;
        }
        movingDoor.localRotation = destination;
    }

    IEnumerator ReopenDoor()
    {
        yield return TurnDoor(openDoorRotation);
        routine = null;
    }

    void UpdateEscape()
    {
        if (!escapeArmed || player == null || playAfter == null || playAfter.movingDoor == null) return;
        var hinge = playAfter.movingDoor;
        var house = hinge.parent;
        Vector3 position = house.InverseTransformPoint(player.position);
        float threshold = hinge.localPosition.z - .18f;
        Vector3 previous = previousEscapePosition;
        previousEscapePosition = position;

        if (!playAfter.reopenedByPlayer || previous.z <= threshold || position.z > threshold) return;
        float crossingX = Mathf.Lerp(previous.x, position.x,
            (previous.z - threshold) / (previous.z - position.z));
        if (crossingX < hinge.localPosition.x - .15f || crossingX > hinge.localPosition.x + 1.5f) return;

        escapeArmed = false;
        StartCoroutine(PlayEscapeSteps(house.forward));
    }

    IEnumerator PlayEscapeSteps(Vector3 inward)
    {
        const int count = 13;
        foregroundUntil = Mathf.Max(foregroundUntil, Time.time + 4.4f);
        for (int step = 0; step < count; step++)
        {
            var clip = escapeFootsteps[step % escapeFootsteps.Length];
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, player.position + inward * Mathf.Lerp(2.7f, 1.4f, step / 12f)
                    + Vector3.up * .2f, .78f);
            yield return new WaitForSeconds(Mathf.Lerp(.30f, .22f, step / 12f));
        }
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
        if (reopenableDoor && movingDoor != null)
        {
            openDoorRotation = movingDoor.localRotation;
            yield return TurnDoor(openDoorRotation * Quaternion.Euler(0, doorAngle, 0));
            if (lowPass != null) lowPass.cutoffFrequency = 19000;
            source.volume = level;
            source.Play();
            doorClosed = true;
            routine = null;
            yield break;
        }

        bool revealOnCut = windowFigure != null && revealFigureOnAudioCut;
        if (foreground) foregroundUntil = Time.time + source.clip.length + 1.25f;
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
                revealOnCut ? 1 : Mathf.Clamp01((source.clip.length - elapsed) / .18f));
            source.volume = level * envelope * (IsOccluded ? .65f : 1);
            if (windowFigure != null && !revealOnCut)
            {
                bool visible = elapsed >= figureDelay && elapsed < figureDelay + figureDuration;
                if (windowFigure.activeSelf != visible) windowFigure.SetActive(visible);
            }
            if (movingDoor != null)
                movingDoor.localRotation = initialDoor * Quaternion.Euler(0,
                    doorAngle * Mathf.SmoothStep(0, 1, elapsed / (doorTravelSeconds > 0 ? doorTravelSeconds : source.clip.length)), 0);
            yield return null;
        }
        if (movingDoor != null) movingDoor.localRotation = initialDoor * Quaternion.Euler(0, doorAngle, 0);
        source.Stop();
        if (revealOnCut)
        {
            windowFigure.SetActive(true);
            if (escapeFootsteps != null && escapeFootsteps.Length > 0 && playAfter != null
                && playAfter.movingDoor != null && playAfter.movingDoor.parent != null)
            {
                warningUntil = Time.time + 2f;
                previousEscapePosition = playAfter.movingDoor.parent.InverseTransformPoint(player.position);
                escapeArmed = true;
            }
            yield return new WaitForSeconds(figureDuration);
        }
        if (windowFigure != null) windowFigure.SetActive(false);
        routine = null;
    }

    void OnDisable()
    {
        if (windowFigure != null) windowFigure.SetActive(false);
        escapeArmed = false;
        warningUntil = 0;
        StopAllCoroutines();
        if (source != null) source.Stop();
        routine = null;
    }
}
