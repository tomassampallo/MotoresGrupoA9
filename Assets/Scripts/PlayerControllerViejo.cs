using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerViejo : MonoBehaviour
{
    [Header("Velocidad")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 5.5f;
    public float rotationSpeed = 10f;

    [Header("Salto y gravedad")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Aterrizaje")]
    
    public float heavyLandingThreshold = -10f;
    
    public float lightLandingThreshold = -4f;
    
    public float rollSpeed = 4f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;

    private bool isGrounded;
    private bool wasGrounded = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        
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
        // La velocidad va bajando a medida que avanza el clip (1 al arrancar, 0 al terminar), para que frene suave.
        if (stateInfo.IsTag("Roll") && stateInfo.normalizedTime < 1f)
        {
            float rollFade = Mathf.Max(1f - stateInfo.normalizedTime, 0.4f); // nunca baja de 40% de impulso
            controller.Move(transform.forward * rollSpeed * rollFade * Time.deltaTime);
            return;
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // lo mantiene pegado al piso
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
            if (Camera.main != null)
            {
                targetAngle += Camera.main.transform.eulerAngles.y;
            }

            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            Vector3 moveDirection = targetRotation * Vector3.forward;
            controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
        }

        float speedPercent = inputDirection.magnitude * (isRunning ? 1f : 0.5f);
        animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);
    }

    void HandleJumpAndGravity(float impactVelocityY)
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");

            animator.SetBool("IsLandingHeavy", false);
            animator.SetBool("IsRolling", false);
        }

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
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            animator.SetTrigger("Slide");
        }

        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Attack");
        }
    }
}
