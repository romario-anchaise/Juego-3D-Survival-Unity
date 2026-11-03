using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using Unity.Cinemachine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(-20)]
[RequireComponent(typeof(CharacterController), typeof(StarterAssetsInputs))]
public class PlayerController3D : MonoBehaviour
{
    public float moveSpeed = 2.1f;
    public float sprintSpeed = 4.4f;
    public float jumpHeight = 1.2f;
    public float gravity = -15f;
    public float acceleration = 10f;
    public float turnSmoothTime = .12f;

    public Transform cameraTarget;

    [Tooltip("Asigna el Animator de Scavenger Variant.")]
    public Animator characterAnimator;

    [Header("Cambio de vista (V)")]
    public CinemachineCamera firstPersonCamera;
    public Vector3 firstPersonEyeOffset = new Vector3(0, 1.65f, 0);
    public bool IsFirstPerson { get; private set; }

    public bool lockPointerOnStart = true;
    public float mouseSensitivity = .12f;

    public AudioClip[] footsteps;
    public AudioClip landing;

    public bool Grounded { get; private set; }
    public float VerticalSpeed { get; private set; }
    public float CurrentSpeed { get; private set; }

    CharacterController controller;
    StarterAssetsInputs input;
    PlayerInput playerInput;
    Animator animator;
    Camera view;
    Renderer[] characterRenderers = new Renderer[0];
    ShadowCastingMode[] originalShadowModes = new ShadowCastingMode[0];

    float yaw;
    float pitch = 8f;
    float turnVelocity;
    Vector3 lastMoveDirection;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<StarterAssetsInputs>();
        playerInput = GetComponent<PlayerInput>();

        // Usa el Animator asignado; conserva soporte para el jugador original.
        animator = characterAnimator != null
            ? characterAnimator
            : GetComponent<Animator>();

        view = Camera.main;
        yaw = transform.eulerAngles.y;
        lastMoveDirection = transform.forward;
        characterRenderers = animator != null
            ? animator.GetComponentsInChildren<Renderer>(true)
            : new Renderer[0];
        originalShadowModes = new ShadowCastingMode[characterRenderers.Length];
        for (int i = 0; i < characterRenderers.Length; i++)
            originalShadowModes[i] = characterRenderers[i].shadowCastingMode;
    }

    void Start()
    {
        if (lockPointerOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (firstPersonCamera != null && Cursor.lockState == CursorLockMode.Locked
            && Time.timeScale > 0 && Keyboard.current != null
            && Keyboard.current.vKey.wasPressedThisFrame)
            SetFirstPerson(!IsFirstPerson);

        UpdateLook();
        Grounded = controller.isGrounded;

        if (Grounded && VerticalSpeed < 0)
            VerticalSpeed = -2f;

        Vector2 axes = input.move;
        Vector3 direction = new Vector3(axes.x, 0, axes.y);

        if (IsFirstPerson || view != null)
        {
            direction = Quaternion.Euler(
                0, IsFirstPerson ? yaw : view.transform.eulerAngles.y, 0
            ) * direction;
        }

        direction = Vector3.ClampMagnitude(direction, 1);

        float desired = direction.magnitude
            * (input.sprint ? sprintSpeed : moveSpeed);

        CurrentSpeed = Mathf.MoveTowards(
            CurrentSpeed,
            desired,
            acceleration * Time.deltaTime
        );

        if (IsFirstPerson)
        {
            transform.rotation = Quaternion.Euler(0, yaw, 0);
        }
        else if (direction.sqrMagnitude > .001f)
        {
            float angle = Mathf.Atan2(direction.x, direction.z)
                * Mathf.Rad2Deg;

            float rotation = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                angle,
                ref turnVelocity,
                turnSmoothTime
            );

            transform.rotation = Quaternion.Euler(0, rotation, 0);
        }

        bool jump = Grounded && input.jump;

        if (jump)
            VerticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);

        input.jump = false;

        VerticalSpeed = Mathf.Max(
            -45f,
            VerticalSpeed + gravity * Time.deltaTime
        );

        if (direction.sqrMagnitude > .001f)
            lastMoveDirection = direction.normalized;
        Vector3 horizontal = lastMoveDirection;

        CollisionFlags collision = controller.Move(
            (horizontal * CurrentSpeed + Vector3.up * VerticalSpeed)
            * Time.deltaTime
        );

        if ((collision & CollisionFlags.Above) != 0 && VerticalSpeed > 0)
            VerticalSpeed = 0;

        Grounded = controller.isGrounded;

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetFloat("Speed", CurrentSpeed, .12f, Time.deltaTime);
            animator.SetFloat("MotionSpeed", 1);
            animator.SetBool("Grounded", Grounded);
            animator.SetBool("Jump", !Grounded && VerticalSpeed > 0);
            animator.SetBool("FreeFall", !Grounded && VerticalSpeed <= 0);
        }
    }

    void LateUpdate()
    {
        if (cameraTarget != null)
            cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0);

        if (firstPersonCamera != null)
            firstPersonCamera.transform.SetPositionAndRotation(
                transform.TransformPoint(firstPersonEyeOffset),
                Quaternion.Euler(pitch, yaw, 0));
    }

    void UpdateLook()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            bool mouse = playerInput != null
                && playerInput.currentControlScheme == "KeyboardMouse";

            if (mouse && Mouse.current != null)
            {
                Vector2 look = Mouse.current.delta.ReadValue()
                    * mouseSensitivity;

                yaw += look.x;
                pitch = Mathf.Clamp(pitch - look.y, IsFirstPerson ? -80 : -35, IsFirstPerson ? 80 : 65);
            }
            else
            {
                Vector2 look = input.look * (110f * Time.deltaTime);

                yaw += look.x;
                pitch = Mathf.Clamp(pitch + look.y, IsFirstPerson ? -80 : -35, IsFirstPerson ? 80 : 65);
            }
        }

    }

    public void SetFirstPerson(bool enabled)
    {
        if (!Application.isPlaying || firstPersonCamera == null)
            return;

        IsFirstPerson = enabled;
        if (!enabled)
            pitch = Mathf.Clamp(pitch, -35, 65);
        else
            transform.rotation = Quaternion.Euler(0, yaw, 0);

        LateUpdate();
        // Supera la prioridad del porche mientras se usa primera persona.
        firstPersonCamera.Priority = 100;
        firstPersonCamera.gameObject.SetActive(enabled);
        for (int i = 0; i < characterRenderers.Length; i++)
            if (characterRenderers[i] != null)
                characterRenderers[i].shadowCastingMode = enabled
                    ? ShadowCastingMode.ShadowsOnly : originalShadowModes[i];
    }

    void OnDisable()
    {
        SetFirstPerson(false);
    }

    void OnFootstep(AnimationEvent e)
    {
        if (e.animatorClipInfo.weight > .5f
            && footsteps != null
            && footsteps.Length > 0)
        {
            AudioClip clip = footsteps[Random.Range(0, footsteps.Length)];

            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, transform.position, .15f);
        }
    }

    void OnLand(AnimationEvent e)
    {
        if (e.animatorClipInfo.weight > .5f && landing != null)
            AudioSource.PlayClipAtPoint(landing, transform.position, .2f);
    }
}
