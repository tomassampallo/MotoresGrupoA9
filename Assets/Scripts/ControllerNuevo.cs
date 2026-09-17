using UnityEngine;

// Requiere que el mismo GameObject tenga un CharacterController.
// Unity lo agrega automáticamente si no lo tenías puesto.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Velocidad")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 5.5f;
    public float rotationSpeed = 10f;

    [Header("Salto y gravedad")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Aterrizaje")]
    [Tooltip("Si velocity.y es menor a este número al tocar el piso, hace el Land pesado en vez del Roll")]
    public float heavyLandingThreshold = -10f;
    [Tooltip("Si velocity.y es menor a este número (pero no llega al de Land pesado), hace el Roll. Por encima de este valor, no dispara ninguna animación especial y sigue corriendo derecho.")]
    public float lightLandingThreshold = -4f;
    [Tooltip("Velocidad a la que avanza automáticamente el personaje mientras dura la animación de Roll")]
    public float rollSpeed = 4f;

    private CharacterController controller;
    private Animator animator;
    private PlayerInputActions inputActions; // clase generada por el Input Actions Asset (la usa también CombatController)
    private Transform cameraTransform;

    private Vector2 moveInput;
    private bool sprintHeld;
    private bool jumpPressed;

    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded = true;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        inputActions = new PlayerInputActions();
        cameraTransform = Camera.main != null ? Camera.main.transform : null;
    }

    void OnEnable()
    {
        inputActions.Enable();

        // Nos suscribimos a los eventos del Input System en vez de leer el input directamente en Update.
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Sprint.performed += ctx => sprintHeld = true;
        inputActions.Player.Sprint.canceled += ctx => sprintHeld = false;

        inputActions.Player.Jump.performed += ctx => jumpPressed = true;
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Update()
    {
        // ---- ÚNICO lugar donde se actualiza isGrounded/wasGrounded en todo el script ----
        // Importante: wasGrounded se guarda ANTES de leer el nuevo valor,
        // así representa realmente "cómo estaba en el frame anterior".
        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        // Guardamos la velocidad de caída ANTES de que HandleMovement la resetee a -2f al tocar el piso.
        // Así usamos el valor real de impacto para decidir Land pesado vs Roll, no el valor ya "limpiado".
        float impactVelocityY = velocity.y;

        HandleMovement();
        HandleJumpAndGravity(impactVelocityY);
        HandleOtherActions();
    }

    void HandleMovement()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Aterrizaje pesado: se frena del todo, no responde a las teclas
        if (stateInfo.IsTag("Land"))
        {
            animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
            return;
        }

        // Roll: avanza solo, automáticamente, sin leer el input del jugador,
        // pero solo mientras el CLIP en sí sigue reproduciéndose (no durante el crossfade de salida).
        // La velocidad va bajando a medida que avanza el clip, con un piso de 40% para que no frene en seco.
        if (stateInfo.IsTag("Roll") && stateInfo.normalizedTime < 1f)
        {
            float rollFade = Mathf.Max(1f - stateInfo.normalizedTime, 0.4f);
            controller.Move(transform.forward * rollSpeed * rollFade * Time.deltaTime);
            return;
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // lo mantiene pegado al piso
        }

        // Movimiento relativo a cámara: "adelante" siempre es hacia donde mira la cámara,
        // no hacia donde mira el personaje (útil con una cámara orbital tipo Cinemachine).
        Vector3 camForward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        Vector3 camRight = cameraTransform != null ? cameraTransform.right : transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;
        float currentSpeed = sprintHeld ? runSpeed : walkSpeed;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        float speedPercent = moveDirection.magnitude * (sprintHeld ? 1f : 0.5f);
        animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);
    }

    void HandleJumpAndGravity(float impactVelocityY)
    {
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");

            animator.SetBool("IsLandingHeavy", false);
            animator.SetBool("IsRolling", false);
        }
        jumpPressed = false;

        // Se cumple UNA sola vez, justo en el frame exacto en que pasa de aire a piso
        if (!wasGrounded && isGrounded)
        {
            if (impactVelocityY < heavyLandingThreshold)
            {
                // Caída muy alta: Land pesado, se frena
                animator.SetBool("IsLandingHeavy", true);
                animator.SetBool("IsRolling", false);
            }
            else if (impactVelocityY < lightLandingThreshold)
            {
                // Caída media: Roll, mantiene el movimiento
                animator.SetBool("IsRolling", true);
                animator.SetBool("IsLandingHeavy", false);
            }
            else
            {
                // Caída chiquita (salto normal de rutina): nada especial, sigue corriendo derecho
                animator.SetBool("IsLandingHeavy", false);
                animator.SetBool("IsRolling", false);
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        animator.SetBool("IsGrounded", isGrounded);
    }

    void HandleOtherActions()
    {
        // TODO: todavía usa el Input Manager viejo porque no sé si ya tenés una acción
        // "Slide" armada en el Input Actions Asset. Si la agregás, la cambiamos para que
        // sea consistente con Move/Sprint/Jump. Por ahora esto sigue funcionando porque
        // dejamos "Active Input Handling" en "Both" en Project Settings.
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            animator.SetTrigger("Slide");
        }

        // El ataque en sí (el daño, el OverlapSphere) ya lo maneja CombatController por su cuenta,
        // suscrito a la misma acción "Attack". Acá solo disparamos la animación.
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Attack");
        }
    }
}