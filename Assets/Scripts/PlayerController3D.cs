using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

// Semana 9: movimiento cinemático, gravedad manual y salto con altura en metros.
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
    public bool lockPointerOnStart = true;
    public float mouseSensitivity = .12f;
    public AudioClip[] footsteps;
    public AudioClip landing;
    public bool Grounded { get; private set; }
    public float VerticalSpeed { get; private set; }
    public float CurrentSpeed { get; private set; }
    CharacterController controller;
    StarterAssetsInputs input;
    Animator animator;
    Camera view;
    float yaw, pitch = 8f, turnVelocity;
    void Awake()
    {
        controller=GetComponent<CharacterController>(); input=GetComponent<StarterAssetsInputs>();
        animator=GetComponent<Animator>(); view=Camera.main;
        yaw=transform.eulerAngles.y;
    }
    void Start()
    {
        if(lockPointerOnStart) {Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
    }
    void Update()
    {
        // Comprobar el suelo antes de integrar la velocidad vertical.
        Grounded=controller.isGrounded;
        if(Grounded && VerticalSpeed<0) VerticalSpeed=-2f;
        Vector2 axes=input.move;
        // El control de teclado se recibe mediante PlayerInput y el Input System moderno.
        var direction=new Vector3(axes.x,0,axes.y);
        if(view!=null) direction=Quaternion.Euler(0,view.transform.eulerAngles.y,0)*direction;
        direction=Vector3.ClampMagnitude(direction,1);
        float desired=direction.magnitude*(input.sprint?sprintSpeed:moveSpeed);
        CurrentSpeed=Mathf.MoveTowards(CurrentSpeed,desired,acceleration*Time.deltaTime);
        if(direction.sqrMagnitude>.001f)
        {
            float angle=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;
            transform.rotation=Quaternion.Euler(0,Mathf.SmoothDampAngle(transform.eulerAngles.y,angle,ref turnVelocity,turnSmoothTime),0);
        }
        // v = raíz(2gh): la altura del salto se configura sin adivinar la velocidad inicial.
        bool jump=Grounded && input.jump;
        if(jump) VerticalSpeed=Mathf.Sqrt(jumpHeight*-2f*gravity);
        input.jump=false;
        VerticalSpeed=Mathf.Max(-45f,VerticalSpeed+gravity*Time.deltaTime);
        var horizontal=direction.sqrMagnitude>.001f?direction.normalized:transform.forward;
        // Move recibe desplazamiento: velocidad (m/s) por deltaTime (s) = metros este frame.
        var collision=controller.Move((horizontal*CurrentSpeed+Vector3.up*VerticalSpeed)*Time.deltaTime);
        if((collision&CollisionFlags.Above)!=0 && VerticalSpeed>0) VerticalSpeed=0;
        Grounded=controller.isGrounded;
        if(animator!=null && animator.runtimeAnimatorController!=null)
        {
            animator.SetFloat("Speed",CurrentSpeed,.12f,Time.deltaTime);
            animator.SetFloat("MotionSpeed",1);
            animator.SetBool("Grounded",Grounded);
            animator.SetBool("Jump",!Grounded && VerticalSpeed>0);
            animator.SetBool("FreeFall",!Grounded && VerticalSpeed<=0);
        }
    }
    void LateUpdate()
    {
        if(cameraTarget==null) return;
        if(Cursor.lockState==CursorLockMode.Locked)
        {
            bool mouse=GetComponent<PlayerInput>()?.currentControlScheme=="KeyboardMouse";
            var look=input.look*(mouse?mouseSensitivity:110f*Time.deltaTime);
            yaw+=look.x; pitch=Mathf.Clamp(pitch-look.y,-35,65);
        }
        // Mantener la orbita independiente del giro del cuerpo, incluso con el cursor libre.
        cameraTarget.rotation=Quaternion.Euler(pitch,yaw,0);
    }
    // Los clips humanoides importados emiten estos eventos en el objeto del Animator.
    void OnFootstep(AnimationEvent e)
    {
        if(e.animatorClipInfo.weight>.5f && footsteps!=null && footsteps.Length>0)
            AudioSource.PlayClipAtPoint(footsteps[Random.Range(0,footsteps.Length)],transform.position,.15f);
    }
    void OnLand(AnimationEvent e)
    {
        if(e.animatorClipInfo.weight>.5f && landing!=null) AudioSource.PlayClipAtPoint(landing,transform.position,.2f);
    }
}
