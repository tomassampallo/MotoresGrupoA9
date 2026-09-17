using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float sprintSpeed = 5.5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Salto y Gravedad")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Aterrizaje y Roll")]
    [SerializeField] private float heavyLandingThreshold = -10f;
    [SerializeField] private float lightLandingThreshold = -4f;
    [SerializeField] private float rollSpeed = 4f;

    [Header("Detección de Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    // Componentes y referencias
    private CharacterController controller;
    private Animator animator;
    private PlayerInputActions inputActions;
    private BloodMeter bloodMeter;
    private Transform cameraTransform;


    private Vector2 moveInput;
    private bool sprintHeld;
    private bool jumpPressed;

 
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        bloodMeter = GetComponent<BloodMeter>();
        inputActions = new PlayerInputActions();
        cameraTransform = Camera.main != null ? Camera.main.transform : null;
    }

    private void OnEnable()
    {
        inputActions.Enable();

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

        float impactVelocityY = velocity.y;

        HandleMovement();
        HandleJumpAndGravity(impactVelocityY);
    }

    private void CheckGrounded()
    {
        wasGrounded = isGrounded;

        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            isGrounded = controller.isGrounded;
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleMovement()
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // aterrizaje pesado, el personaje se frena completamente
            if (stateInfo.IsTag("Land"))
            {
                animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
                return;
            }

            // roll, movimiento automatico con desaceleración
            if (stateInfo.IsTag("Roll") && stateInfo.normalizedTime < 1f)
            {
                float rollFade = Mathf.Max(1f - stateInfo.normalizedTime, 0.4f);
                controller.Move(transform.forward * rollSpeed * rollFade * Time.deltaTime);
                return;
            }
        }

        // Dirección de movimiento orientada a la camara
        Vector3 camForward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        Vector3 camRight = cameraTransform != null ? cameraTransform.right : transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;

        // Factor de debilidad de la mecánica de sangre
        float speedMult = bloodMeter != null ? bloodMeter.GetSpeedMultiplier() : 1f;
        float currentSpeed = (sprintHeld ? sprintSpeed : walkSpeed) * speedMult;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
        }

        // actualizacion de parámetros en Animator
        if (animator != null)
        {
            float speedPercent = moveInput.magnitude * (sprintHeld ? 1f : 0.5f);
            animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);
        }
    }

    private void HandleJumpAndGravity(float impactVelocityY)
    {
        // Salto
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
            if (animator != null)
            {
                animator.SetTrigger("Jump");
                animator.SetBool("IsLandingHeavy", false);
                animator.SetBool("IsRolling", false);
            }
        }
        jumpPressed = false;

        // Evaluación del impacto en el frame exacto del aterrizaje
        if (!wasGrounded && isGrounded)
        {
            if (animator != null)
            {
                if (impactVelocityY < heavyLandingThreshold)
                {
                    animator.SetBool("IsLandingHeavy", true);
                    animator.SetBool("IsRolling", false);
                }
                else if (impactVelocityY < lightLandingThreshold)
                {
                    animator.SetBool("IsRolling", true);
                    animator.SetBool("IsLandingHeavy", false);
                }
                else
                {
                    animator.SetBool("IsLandingHeavy", false);
                    animator.SetBool("IsRolling", false);
                }
            }
        }

        // aplicación de la gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
        }
    }
}
