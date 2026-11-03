using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

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

    float yaw;
    float pitch = 8f;
    float turnVelocity;

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
        Grounded = controller.isGrounded;

        if (Grounded && VerticalSpeed < 0)
            VerticalSpeed = -2f;

        Vector2 axes = input.move;
        Vector3 direction = new Vector3(axes.x, 0, axes.y);

        if (view != null)
        {
            direction = Quaternion.Euler(
                0, view.transform.eulerAngles.y, 0
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

        if (direction.sqrMagnitude > .001f)
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

        Vector3 horizontal = direction.sqrMagnitude > .001f
            ? direction.normalized
            : transform.forward;

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
        if (cameraTarget == null)
            return;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            bool mouse = playerInput != null
                && playerInput.currentControlScheme == "KeyboardMouse";

            if (mouse && Mouse.current != null)
            {
                Vector2 look = Mouse.current.delta.ReadValue()
                    * mouseSensitivity;

                yaw += look.x;
                pitch = Mathf.Clamp(pitch - look.y, -35, 65);
            }
            else
            {
                Vector2 look = input.look * (110f * Time.deltaTime);

                yaw += look.x;
                pitch = Mathf.Clamp(pitch + look.y, -35, 65);
            }
        }

        cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0);
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