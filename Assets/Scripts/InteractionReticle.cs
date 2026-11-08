using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Image))]
public sealed class InteractionReticle : MonoBehaviour
{
    [SerializeField] GameObject[] readingPages;
    UnityEngine.UI.Image dot;

    void Awake() => dot = GetComponent<UnityEngine.UI.Image>();

    void LateUpdate()
    {
        bool visible = Cursor.lockState == CursorLockMode.Locked && Time.timeScale > 0;
        if (readingPages != null)
            foreach (var page in readingPages)
                if (page != null && page.activeInHierarchy) visible = false;
        dot.enabled = visible;
    }

    void OnDisable()
    {
        if (dot != null) dot.enabled = false;
    }
}
