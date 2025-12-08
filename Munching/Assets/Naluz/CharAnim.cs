using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // --- Public Variables (Adjustable in the Inspector) ---
    public float baseWalkSpeed = 5.0f; // Speed when WASD is pressed (Walking)
    public float sprintSpeedMultiplier = 2.0f; // Multiplier for speed when sprinting (Shift)
    public float rotationSpeed = 500f;

    // --- Private Variables ---
    private Rigidbody rb;
    private Animator animator;

    // Animator Parameter Names (MUST match the setup in the Animator Controller)
    private const string IS_MOVING_PARAM = "IsMoving"; // Boolean for Idle/Walk toggle
    private const string EAT_TRIGGER = "Aj@Munching"; 
    private const string SIT_TRIGGER = "Aj@Sitting"; 
    private const string COLLECT_TRIGGER = "Aj@Picking"; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on Aj. Movement will not work.");
        }
        if (animator == null)
        {
            Debug.LogWarning("Animator component not found on Aj. Animation controls will not work.");
        }
    }

    void Update()
    {
        HandleMovementInput();
        HandleActionAnimationInput();
    }

    // --- Movement and Animation Toggle ---
    void HandleMovementInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the direction vector based on input
        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;

        float currentSpeed;

        // --- Speed Control (Shift) ---
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            // Sprinting speed for physics movement
            currentSpeed = baseWalkSpeed * sprintSpeedMultiplier;
        }
        else
        {
            // Walking speed for physics movement
            currentSpeed = baseWalkSpeed;
        }

        // Apply movement velocity to the Rigidbody
        Vector3 finalVelocity = moveDirection * currentSpeed;
        rb.linearVelocity = new Vector3(finalVelocity.x, rb.linearVelocity.y, finalVelocity.z);

        // --- Rotation ---
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // --- Animation Control (Boolean Toggle) ---
        if (animator != null)
        {
            // True if WASD is pressed (magnitude > 0.1), False if standing still
            bool isMoving = moveDirection.magnitude > 0.1f; 
            
            // Set the Boolean parameter in the Animator
            animator.SetBool(IS_MOVING_PARAM, isMoving);
        }
    }

    // --- One-Off Action Animations (E, C, Q) ---
    void HandleActionAnimationInput()
    {
        if (animator == null) return; 

        // E Key: Aj@Munching
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger(EAT_TRIGGER);
        }

        // C Key: Aj@Sitting
        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.SetTrigger(SIT_TRIGGER);
        }

        // Q Key: Aj@Picking
        if (Input.GetKeyDown(KeyCode.Q))
        {
            animator.SetTrigger(COLLECT_TRIGGER);
        }
    }
}