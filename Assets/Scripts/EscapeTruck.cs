using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public sealed class EscapeTruck : MonoBehaviour
{
    [SerializeField] Collider driverDoor;
    [SerializeField] CinemachineCamera cabinCamera;
    [SerializeField] Transform wheel;
    [SerializeField] Transform[] tires;
    [SerializeField] Light[] headlights;

    const int World = 1;
    const float StartDelay = 1.6f;
    Rigidbody body;
    PlayerController3D driver;
    CharacterController driverCollider;
    Renderer[] driverRenderers;
    bool[] renderStates;
    Quaternion wheelRest;
    Quaternion[] tireRest;
    float heading, speed, yaw, pitch, tireAngle, startedAt, steering;
    bool wasFirstPerson;
    PlayerInventory inventory;

    public bool Occupied => driver != null;
    public bool Starting => Occupied && Time.time < startedAt + StartDelay;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        inventory = FindFirstObjectByType<PlayerInventory>();
        heading = transform.eulerAngles.y;
        wheelRest = wheel.localRotation;
        tireRest = new Quaternion[tires.Length];
        for (int i = 0; i < tires.Length; i++)
            tireRest[i] = tires[i].localRotation;
    }

    public bool CanBoard(PlayerInventory inventory) =>
        !Occupied && InteractionAim.IsTargeted(driverDoor, inventory.transform, 2.1f);

    public void Board(PlayerInventory inventory)
    {
        if (Occupied) return;
        driver = inventory.GetComponent<PlayerController3D>();
        if (driver == null) return;
        wasFirstPerson = driver.IsFirstPerson;
        driver.enabled = false;
        driverCollider = driver.GetComponent<CharacterController>();
        driverCollider.enabled = false;
        driverRenderers = driver.GetComponentsInChildren<Renderer>();
        renderStates = new bool[driverRenderers.Length];
        for (int i = 0; i < driverRenderers.Length; i++)
        {
            renderStates[i] = driverRenderers[i].enabled;
            driverRenderers[i].enabled = false;
        }
        yaw = pitch = 0;
        startedAt = Time.time;
        cabinCamera.transform.localRotation = Quaternion.identity;
        cabinCamera.gameObject.SetActive(true);
        driver.transform.position = transform.position;
    }

    public void Leave()
    {
        if (!Occupied || Mathf.Abs(speed) > .35f) return;
        Vector3 side = transform.TransformPoint(-1.7f, 0, .55f);
        if (!Ground(side, out var ground)) return;
        Vector3 feet = ground.point + Vector3.up * .05f;
        if (Physics.CheckCapsule(feet + Vector3.up * .35f, feet + Vector3.up * 1.5f,
            .27f, World, QueryTriggerInteraction.Ignore)) return;

        driver.transform.position = feet;
        driverCollider.enabled = true;
        for (int i = 0; i < driverRenderers.Length; i++)
            if (driverRenderers[i] != null) driverRenderers[i].enabled = renderStates[i];
        cabinCamera.gameObject.SetActive(false);
        driver.enabled = true;
        driver.SetFirstPerson(wasFirstPerson);
        driver = null;
        speed = 0;
        foreach (var light in headlights) light.enabled = false;
    }

    void Update()
    {
        if (Time.timeScale <= 0 || Cursor.lockState != CursorLockMode.Locked) return;
        var keys = Keyboard.current;
        if (keys != null && keys.eKey.wasPressedThisFrame && !BedroomWarningNote.InputConsumedThisFrame)
        {
            if (Occupied) Leave();
            else if (inventory != null && CanBoard(inventory)) Board(inventory);
        }
        if (!Occupied || Cursor.lockState != CursorLockMode.Locked || Mouse.current == null) return;
        Vector2 look = Mouse.current.delta.ReadValue() * .11f;
        yaw = Mathf.Clamp(yaw + look.x, -100f, 100f);
        pitch = Mathf.Clamp(pitch - look.y, -35f, 32f);
        cabinCamera.transform.localRotation = Quaternion.Euler(pitch, yaw, 0);
    }

    void FixedUpdate()
    {
        if (!Occupied) return;
        foreach (var light in headlights) light.enabled = !Starting;
        var keys = Keyboard.current;
        bool control = !Starting && keys != null && Cursor.lockState == CursorLockMode.Locked;
        float throttle = control ? (keys.wKey.isPressed ? 1f : 0f) - (keys.sKey.isPressed ? 1f : 0f) : 0f;
        float turn = control ? (keys.dKey.isPressed ? 1f : 0f) - (keys.aKey.isPressed ? 1f : 0f) : 0f;
        float targetSpeed = throttle > 0 ? 6.5f : throttle < 0 ? -2.6f : 0f;
        Drive(control && keys.spaceKey.isPressed ? 0f : targetSpeed, turn);
    }

    void Drive(float targetSpeed, float turn)
    {
        float step = Time.fixedDeltaTime;
        speed = Mathf.MoveTowards(speed, targetSpeed, (targetSpeed == 0 ? 7f : 3.2f) * step);
        steering = Mathf.MoveTowards(steering, turn, 5f * step);
        float nextHeading = heading + steering * Mathf.Clamp(speed, -3f, 4f) * 13f * step;
        Vector3 forward = Quaternion.Euler(0, nextHeading, 0) * Vector3.forward;
        Vector3 next = body.position + forward * (speed * step);
        if (!Ground(next - forward * .9f, out var rear) || !Ground(next + forward * 1.7f, out var front))
        {
            speed = 0;
            return;
        }
        Vector3 slope = (front.point - rear.point).normalized;
        if (Vector3.Angle(slope, forward) > 27f)
        {
            speed = 0;
            return;
        }
        next.y = Mathf.Lerp(rear.point.y, front.point.y, .9f / 2.6f) + .045f;
        Quaternion rotation = Quaternion.LookRotation(slope, Vector3.up);
        Vector3 offset = new Vector3(0, 1.05f, .22f);
        Vector3 center = body.position + body.rotation * offset;
        Vector3 move = next + rotation * offset - center;
        if (move.sqrMagnitude > .000001f && Physics.BoxCast(center, new Vector3(.86f, .60f, 2.1f),
            move.normalized, out _, body.rotation, move.magnitude + .04f, World, QueryTriggerInteraction.Ignore))
        {
            speed = 0;
            return;
        }
        heading = nextHeading;
        body.MovePosition(next);
        body.MoveRotation(rotation);
        driver.transform.position = next;
        wheel.localRotation = wheelRest * Quaternion.AngleAxis(steering * -90f, Vector3.up);
        tireAngle += speed * step / .36f * Mathf.Rad2Deg;
        for (int i = 0; i < tires.Length; i++)
            tires[i].localRotation = tireRest[i] * Quaternion.AngleAxis(tireAngle, Vector3.right);
    }

    bool Ground(Vector3 point, out RaycastHit hit)
    {
        return Physics.Raycast(point + Vector3.up * 3, Vector3.down, out hit, 6f,
            World, QueryTriggerInteraction.Ignore) && hit.collider is TerrainCollider;
    }
}
