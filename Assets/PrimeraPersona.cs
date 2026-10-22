using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PrimeraPersona : MonoBehaviour
{
    public Transform camara;
    public float velocidad = 3.5f;
    public float aceleracion = 12f;
    public float sensibilidad = 0.12f;

    private CharacterController controlador;
    private Vector3 movimiento;
    private float velocidadVertical;
    private float inclinacion;

    void Start()
    {
        controlador = GetComponent<CharacterController>();

        // Buscar la cámara hija si no se asignó en el Inspector.
        if (camara == null)
        {
            Camera camaraHija = GetComponentInChildren<Camera>();

            if (camaraHija != null)
                camara = camaraHija.transform;
        }

        if (camara == null)
        {
            Debug.LogError(
                "Falta una cámara dentro de Jugador o asignada al campo Camara.",
                this
            );

            enabled = false;
            return;
        }

        BloquearMouse();
    }

    void Update()
    {
        Keyboard teclado = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (teclado != null && teclado.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            BloquearMouse();
        }

        Vector3 objetivo = Vector3.zero;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // Mirar con el mouse.
            if (mouse != null)
            {
                Vector2 mirada = mouse.delta.ReadValue() * sensibilidad;

                transform.Rotate(0f, mirada.x, 0f);

                inclinacion -= mirada.y;
                inclinacion = Mathf.Clamp(inclinacion, -85f, 85f);

                camara.localRotation =
                    Quaternion.Euler(inclinacion, 0f, 0f);
            }

            // Caminar con WASD.
            if (teclado != null)
            {
                float horizontal =
                    (teclado.dKey.isPressed ? 1f : 0f) -
                    (teclado.aKey.isPressed ? 1f : 0f);

                float vertical =
                    (teclado.wKey.isPressed ? 1f : 0f) -
                    (teclado.sKey.isPressed ? 1f : 0f);

                Vector3 direccion =
                    transform.right * horizontal +
                    transform.forward * vertical;

                direccion = Vector3.ClampMagnitude(direccion, 1f);
                objetivo = direccion * velocidad;
            }
        }

        // Acelerar y frenar suavemente.
        movimiento = Vector3.MoveTowards(
            movimiento,
            objetivo,
            aceleracion * Time.deltaTime
        );

        // Gravedad y contacto con el suelo.
        if (controlador.isGrounded && velocidadVertical < 0f)
            velocidadVertical = -2f;

        velocidadVertical += Physics.gravity.y * Time.deltaTime;

        Vector3 desplazamiento =
            movimiento + Vector3.up * velocidadVertical;

        CollisionFlags colisiones =
            controlador.Move(desplazamiento * Time.deltaTime);

        if ((colisiones & CollisionFlags.Above) != 0 &&
            velocidadVertical > 0f)
        {
            velocidadVertical = 0f;
        }
    }

    void BloquearMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}