using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(200)]
public sealed class PlayerFlashlight : MonoBehaviour
{
    [SerializeField] PlayerController3D player;
    [SerializeField] PlayerInventory inventory;
    [SerializeField] Transform rightHand;
    [SerializeField] Transform flashlightModel;
    [SerializeField] Light beam;
    public bool IsOn { get; private set; }
    public bool IsFailing { get; private set; }
    float normalIntensity, untilFault, faultTime, draw, blackoutDuration;
    readonly System.Random random = new System.Random();

    void Awake()
    {
        normalIntensity = beam != null ? beam.intensity : 65;
        ScheduleFault();
    }
    void ScheduleFault() => untilFault = 18 + (float)random.NextDouble() * 14;
    public void Toggle()
    {
        if (inventory == null || !inventory.HasFlashlight) return;
        IsOn = !IsOn;
        IsFailing = false;
        faultTime = 0;
        ScheduleFault();
        if (beam != null) beam.enabled = IsOn;
    }
    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked && Time.timeScale > 0
            && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) Toggle();
        if (!IsOn) return;
        if (!IsFailing)
        {
            untilFault -= Time.deltaTime;
            if (untilFault <= 0)
            {
                IsFailing = true;
                faultTime = 0;
                blackoutDuration = .8f + (float)random.NextDouble() * .5f;
            }
        }
        else
        {
            faultTime += Time.deltaTime;
            if (faultTime >= .18f + blackoutDuration + .35f)
            { IsFailing = false; ScheduleFault(); }
        }
    }
    float Brightness()
    {
        if (!IsFailing) return 1;
        if (faultTime < .18f) return .15f;
        if (faultTime < .18f + blackoutDuration) return 0;
        return Mathf.SmoothStep(.1f, 1, (faultTime - .18f - blackoutDuration) / .35f);
    }
    void LateUpdate()
    {
        var view = Camera.main;
        if (player == null || view == null || flashlightModel == null || beam == null) return;
        if (inventory == null || !inventory.HasFlashlight)
        {
            flashlightModel.gameObject.SetActive(false);
            beam.enabled = false;
            return;
        }
        draw = Mathf.MoveTowards(draw, IsOn ? 1 : 0, Time.deltaTime * 5);
        Vector3 belt = transform.TransformPoint(new Vector3(.20f,.92f,-.10f));
        Quaternion stored = Quaternion.LookRotation(-transform.up, transform.forward);
        if (draw <= 0 && !IsOn)
        {
            flashlightModel.gameObject.SetActive(!player.IsFirstPerson);
            flashlightModel.SetPositionAndRotation(belt, stored);
            beam.enabled = false;
            return;
        }
        flashlightModel.gameObject.SetActive(true);
        Ray sight = view.ViewportPointToRay(new Vector3(.5f,.5f,0));
        Vector3 target = sight.GetPoint(22);
        if (Physics.Raycast(sight, out var hit, 22, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) target = hit.point;
        Vector3 eye = transform.position + Vector3.up * 1.5f;
        Vector3 position = player.IsFirstPerson
            ? view.transform.TransformPoint(Vector3.Lerp(new Vector3(.20f,-.55f,.42f),new Vector3(.20f,-.21f,.48f),draw))
            : Vector3.Lerp(belt, rightHand != null ? rightHand.position : eye, draw);
        if (Physics.Linecast(eye, position, out var wall, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) position = wall.point + (eye-wall.point).normalized*.04f;
        Vector3 direction = target - position;
        if (direction.sqrMagnitude < .01f) direction = sight.direction;
        flashlightModel.SetPositionAndRotation(position, player.IsFirstPerson
            ? view.transform.rotation * Quaternion.Euler((1-draw)*40,0,0)
            : Quaternion.Slerp(stored,Quaternion.LookRotation(direction),draw));
        Vector3 emitter = position + flashlightModel.forward*.13f;
        if (Physics.Linecast(eye, emitter, out wall, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) emitter = wall.point+(eye-wall.point).normalized*.04f;
        beam.transform.SetPositionAndRotation(emitter, Quaternion.LookRotation(target-emitter));
        float brightness = Brightness();
        beam.intensity = normalIntensity * brightness;
        beam.enabled = IsOn && brightness > .01f;
    }
    void OnDisable() { if (beam != null) beam.enabled = false; }
}
