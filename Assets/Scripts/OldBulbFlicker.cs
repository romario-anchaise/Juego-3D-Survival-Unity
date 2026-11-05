using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Light))]
public class OldBulbFlicker : MonoBehaviour
{
    [Min(0)] public float baseIntensity = .65f;
    public Vector2 intervalSeconds = new Vector2(3f, 7f);
    [Range(0, 1)] public float dimMultiplier = .03f;
    public int patternSeed = 17;
    public Renderer bulbRenderer;

    Light bulb;
    Coroutine flicker;
    MaterialPropertyBlock properties;
    Color emission;

    void OnEnable()
    {
        bulb = GetComponent<Light>();
        properties = new MaterialPropertyBlock();
        if (bulbRenderer != null)
            emission = bulbRenderer.sharedMaterial.GetColor("_EmissionColor");
        bulb.intensity = baseIntensity;
        flicker = StartCoroutine(Flicker());
    }

    void LateUpdate()
    {
        if (bulbRenderer == null) return;
        bulbRenderer.GetPropertyBlock(properties);
        properties.SetColor("_EmissionColor", emission *
            (baseIntensity > 0 ? bulb.intensity / baseIntensity : 0));
        bulbRenderer.SetPropertyBlock(properties);
    }

    IEnumerator Flicker()
    {
        // Equal seeds synchronize the porch bulb and its doorway accent.
        var random = new System.Random(patternSeed);
        yield return new WaitForSeconds(1.5f + (float)random.NextDouble());
        while (true)
        {
            bulb.intensity = baseIntensity * dimMultiplier;
            yield return new WaitForSeconds(.22f + (float)random.NextDouble() * .18f);
            bulb.intensity = baseIntensity;
            yield return new WaitForSeconds(.18f + (float)random.NextDouble() * .22f);
            bulb.intensity = 0;
            yield return new WaitForSeconds(.4f + (float)random.NextDouble() * .35f);
            // A dying bulb struggles back instead of snapping to full brightness.
            float recovery = .55f + (float)random.NextDouble() * .4f;
            float elapsed = 0;
            while (elapsed < recovery)
            {
                elapsed += Time.deltaTime;
                float brightness = Mathf.SmoothStep(.15f, 1f, elapsed / recovery);
                bulb.intensity = baseIntensity * brightness;
                yield return null;
            }
            bulb.intensity = baseIntensity;
            float minimum = Mathf.Max(.5f, intervalSeconds.x);
            float maximum = Mathf.Max(minimum, intervalSeconds.y);
            yield return new WaitForSeconds(Mathf.Lerp(minimum, maximum,
                (float)random.NextDouble()));
        }
    }

    void OnDisable()
    {
        if (flicker != null) StopCoroutine(flicker);
        if (bulb != null) bulb.intensity = baseIntensity;
        if (bulbRenderer != null) bulbRenderer.SetPropertyBlock(null);
        flicker = null;
    }
}
