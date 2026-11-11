using UnityEngine;

/// <summary>Quiet stereo bed, crossfaded at the seam and muffled inside buildings.</summary>
[DisallowMultipleComponent]
public sealed class HorrorWindAmbience : MonoBehaviour
{
    public AudioClip wind;
    public Transform player;
    public Bounds[] indoorAreas;
    public WorkshopWatcherEncounter encounter;
    [Range(0, 1)] public float outdoorLevel = .08f;
    [Range(0, 1)] public float indoorMultiplier = .22f;
    [Min(.1f)] public float crossfadeSeconds = 4;
    public bool Indoors { get; private set; }
    public float MixLevel { get; private set; }
    public int CompletedCrossfades { get; private set; }
    AudioSource[] sources;
    AudioLowPassFilter[] filters;
    int current;
    double nextTransition;
    double transitionStart;
    bool transitioning;
    float fadeDuration;

    void OnEnable()
    {
        if (wind == null) return;
        if (sources == null)
        {
            sources = new AudioSource[2];
            filters = new AudioLowPassFilter[2];
            for (int i = 0; i < 2; i++)
            {
                var channel = new GameObject("Viento_" + (i + 1));
                channel.transform.SetParent(transform, false);
                sources[i] = channel.AddComponent<AudioSource>();
                sources[i].playOnAwake = false;
                sources[i].spatialBlend = 0;
                sources[i].dopplerLevel = 0;
                sources[i].priority = 180;
                filters[i] = channel.AddComponent<AudioLowPassFilter>();
                filters[i].cutoffFrequency = 16000;
            }
        }
        fadeDuration = Mathf.Clamp(crossfadeSeconds, .1f, wind.length * .4f);
        foreach (var source in sources) { source.clip = wind; source.volume = 0; }
        current = 0;
        transitioning = false;
        MixLevel = 0;
        sources[current].Play();
        nextTransition = AudioSettings.dspTime + wind.length - fadeDuration;
    }

    void Update()
    {
        if (sources == null || wind == null) return;
        Indoors = false;
        if (player != null && indoorAreas != null)
            foreach (var area in indoorAreas)
                if (area.Contains(player.position + Vector3.up)) { Indoors = true; break; }
        bool encounterActive = encounter != null
            && encounter.CurrentPhase != WorkshopWatcherEncounter.Phase.Dormant
            && encounter.CurrentPhase != WorkshopWatcherEncounter.Phase.Gone;
        float target = outdoorLevel * (Indoors ? indoorMultiplier : 1)
            * (HorrorSoundCue.ForegroundActive || encounterActive ? .5f : 1);
        if (Time.timeScale <= 0) target = 0;
        MixLevel = Mathf.MoveTowards(MixLevel, target, Time.unscaledDeltaTime * outdoorLevel / 2);
        foreach (var filter in filters)
            filter.cutoffFrequency = Mathf.Lerp(filter.cutoffFrequency,
                Indoors ? 2800 : 16000, Time.unscaledDeltaTime * 2);
        double now = AudioSettings.dspTime;
        if (!transitioning && now >= nextTransition)
        {
            sources[1 - current].Play();
            transitionStart = now;
            transitioning = true;
        }
        if (transitioning)
        {
            float t = Mathf.Clamp01((float)(now - transitionStart) / fadeDuration);
            sources[current].volume = MixLevel * (1 - t);
            sources[1 - current].volume = MixLevel * t;
            if (t >= 1)
            {
                sources[current].Stop();
                current = 1 - current;
                transitioning = false;
                CompletedCrossfades++;
                nextTransition = transitionStart + wind.length - fadeDuration;
            }
        }
        else sources[current].volume = MixLevel;
    }

    void OnDisable()
    {
        if (sources != null)
            foreach (var source in sources) if (source != null) source.Stop();
        MixLevel = 0;
    }
}
