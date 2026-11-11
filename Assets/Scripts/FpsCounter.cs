using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(UnityEngine.UI.Text))]
public sealed class FpsCounter : MonoBehaviour
{
    UnityEngine.UI.Text label;
    float elapsed;
    int frames;
    int lastValue = -1;

    void Awake() => label = GetComponent<UnityEngine.UI.Text>();

    void OnEnable()
    {
        elapsed = 0;
        frames = 0;
        lastValue = -1;
        label.text = "FPS --";
    }

    void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        frames++;
        if (elapsed < .25f) return;
        int value = Mathf.RoundToInt(frames / elapsed);
        if (value != lastValue)
        {
            label.text = "FPS " + value;
            lastValue = value;
        }
        frames = 0;
        elapsed = 0;
    }
}
