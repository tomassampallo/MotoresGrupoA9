using UnityEngine;

// Requiere que el mismo GameObject tenga un CharacterController.
// Unity lo agrega automáticamente si no lo tenías puesto.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7.5f; // representa correr por los tejados
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Detección de suelo")]
    [SerializeField] private Transform groundCheck; // objeto vacío ubicado a la altura de los pies
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    private CharacterController controller;
    private PlayerInputActions inputActions; // clase generada por el Input Actions Asset

    private Vector2 moveInput;
    private bool sprintHeld;
    private bool jumpPressed;

    private Vector3 velocity;
    private bool isGrounded;

    private Transform cameraTransform;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();
        cameraTransform = Camera.main != null ? Camera.main.transform : null;
    }

    private void OnEnable()
    {
        inputActions.Enable();

        // Nos suscribimos a los eventos del Input System en vez de leer el input directamente en Update.
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Sprint.performed += ctx => sprintHeld = true;
        inputActions.Player.Sprint.canceled += ctx => sprintHeld = false;

        inputActions.Player.Jump.performed += ctx => jumpPressed = true;
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleJumpAndGravity();
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // lo "pega" al piso para que la gravedad no se acumule de más
        }
    }

    private void HandleMovement()
    {
        // Convertimos el input (horizontal/vertical) en una dirección relativa a la cámara,
        // así "adelante" siempre es hacia donde mira la cámara y no hacia donde mira el personaje.
        Vector3 camForward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        Vector3 camRight = cameraTransform != null ? cameraTransform.right : transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;

        float currentSpeed = sprintHeld ? sprintSpeed : walkSpeed;
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Rotamos al personaje suavemente hacia donde se está moviendo
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleJumpAndGravity()
    {
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        jumpPressed = false;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}