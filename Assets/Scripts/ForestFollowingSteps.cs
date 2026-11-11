using UnityEngine;

/// <summary>Occasional extra footsteps after the player stops outdoors.</summary>
[DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
public sealed class ForestFollowingSteps : MonoBehaviour
{
    public PlayerController3D player;
    public WorkshopWatcherEncounter encounter;
    public GameObject[] readingPages;
    public AudioClip[] steps;
    public Bounds[] indoorAreas;
    public Vector2 interval = new Vector2(40, 65);
    [Range(0, 1)] public float level = .4f;
    public int maximumOccurrences = 4;
    public int TriggerCount { get; private set; }
    AudioSource source;
    float movingTime;
    float stoppedTime;
    float nextAllowed;
    bool armed;
    int lastClip = -1;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        nextAllowed = Time.time + 25;
    }

    bool Indoors()
    {
        if (indoorAreas == null) return false;
        foreach (var area in indoorAreas)
            if (area.Contains(player.transform.position + Vector3.up)) return true;
        return false;
    }

    void Update()
    {
        if (player == null || steps == null || steps.Length == 0) return;
        if (HorrorSoundCue.IsBusy(encounter, readingPages) || HorrorSoundCue.ForegroundActive
            || Indoors() || !player.Grounded)
        {
            movingTime = stoppedTime = 0;
            armed = false;
            return;
        }
        if (player.CurrentSpeed > .5f)
        {
            movingTime += Time.deltaTime;
            stoppedTime = 0;
            if (movingTime > 2) armed = true;
        }
        else
        {
            movingTime = 0;
            stoppedTime += Time.deltaTime;
            if (armed && stoppedTime > .65f && Time.time >= nextAllowed
                && TriggerCount < maximumOccurrences) PlayFollowingStep();
        }
    }

    void PlayFollowingStep()
    {
        var camera = Camera.main;
        Vector3 forward = camera != null ? camera.transform.forward : player.transform.forward;
        forward.y = 0;
        if (forward.sqrMagnitude < .01f) forward = player.transform.forward;
        Vector3 spot = player.transform.position - forward.normalized * Random.Range(3.2f, 4.2f)
            + Vector3.Cross(Vector3.up, forward.normalized) * Random.Range(-1.3f, 1.3f);
        float height = spot.y;
        foreach (var terrain in Terrain.activeTerrains)
        {
            Vector3 local = spot - terrain.transform.position;
            if (local.x >= 0 && local.z >= 0 && local.x <= terrain.terrainData.size.x
                && local.z <= terrain.terrainData.size.z)
                height = terrain.SampleHeight(spot) + terrain.transform.position.y;
        }
        spot.y = height + .25f;
        // A following step must not be placed through a wall or solid obstacle.
        if (Physics.Linecast(player.transform.position + Vector3.up, spot,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            armed = false;
            return;
        }
        transform.position = spot;
        int index = Random.Range(0, steps.Length);
        if (steps.Length > 1 && index == lastClip) index = (index + 1) % steps.Length;
        lastClip = index;
        source.clip = steps[index];
        source.volume = level;
        source.pitch = Random.Range(.96f, 1.02f);
        source.Play();
        TriggerCount++;
        armed = false;
        nextAllowed = Time.time + Random.Range(interval.x, interval.y);
    }

    void OnDisable()
    {
        if (source != null) source.Stop();
    }
}
