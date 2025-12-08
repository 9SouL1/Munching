using UnityEngine;

public class BeginnerCharacterController : MonoBehaviour
{
    // Movement configuration
    public float baseMovementSpeed = 5.0f;          // Base character walk speed
    public float sprintSpeedMultiplier = 2.0f;      // Character sprint speed factor
    public float rotationSpeed = 500f;              // Character rotation speed limit

    // Component references
    private Rigidbody characterRigidbody;           // Character physics body component
    private Animator characterAnimator;             // Character animation controller
    
    // Camera references
    private GameObject thirdPovCamera;              // Third person camera reference
    private GameObject firstPovCamera;               // First person camera reference
    
    // State variables
    private bool isSittingToggled = false;          // True if C key toggled
    private GameObject nearestChair = null;         // Currently detected chair object
    private bool isNearChair = false;               // True if near any chair

    // Chair restoration data
    private Vector3 originalChairPosition;          // Chair's initial world position
    private Quaternion originalChairRotation;       // Chair's initial world rotation

    void Start()
    {
        characterRigidbody = GetComponent<Rigidbody>();
        if (characterRigidbody == null)
        {
            Debug.LogError("🚨 Rigidbody component missing! Add one.");
        }
        characterAnimator = GetComponent<Animator>();

        thirdPovCamera = GameObject.Find("Third Pov Camera");
        firstPovCamera = GameObject.Find("First Pov Camera");

        if (thirdPovCamera == null || firstPovCamera == null)
        {
            Debug.LogError("🚨 Camera objects missing! Check scene names.");
        }
        else
        {
            thirdPovCamera.SetActive(true);
            firstPovCamera.SetActive(false);
        }
    }
    
    void Update()
    {
        HandleMovementAndRotation();
        HandleAnimationInput();
        HandleCameraSwitch();
    }

    void HandleMovementAndRotation()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        float currentSpeed = baseMovementSpeed;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        if (isSprinting)
        {
            currentSpeed *= sprintSpeedMultiplier;
        }

        Vector3 targetVelocity;
        bool isMoving = (verticalInput != 0 || horizontalInput != 0);
        
        if (isMoving)
        {
            Vector3 forwardMovement = transform.forward * verticalInput;
            Vector3 strafeMovement = transform.right * horizontalInput;
            Vector3 movementDirection = (forwardMovement + strafeMovement);

            if (movementDirection.magnitude > 1f) 
            {
                movementDirection.Normalize();
            }
            targetVelocity = movementDirection * currentSpeed;

            if (isSittingToggled)
            {
                isSittingToggled = false;
                RestoreChairPosition();
            }
        }
        else
        {
            targetVelocity = Vector3.zero;
        }
        
        bool isMunchingHeld = isSittingToggled && Input.GetKey(KeyCode.E);

        // Movement applied using Rigidbody.MovePosition for collision safety
        if (isMoving && !isSittingToggled && !isMunchingHeld)
        {
            // Calculate the final position for this frame
            Vector3 movement = new Vector3(targetVelocity.x, 0, targetVelocity.z);
            Vector3 targetPosition = characterRigidbody.position + movement * Time.deltaTime;

            // Apply movement using MovePosition (Preserve current Rigidbody Y position)
            characterRigidbody.MovePosition(new Vector3(
                targetPosition.x,
                characterRigidbody.position.y,
                targetPosition.z
            ));
            
            // Rotation Logic remains the same
            Vector3 rotationDirection = new Vector3(transform.right.x * horizontalInput, 0f, transform.right.z * horizontalInput);
            if (verticalInput > 0) 
            {
                rotationDirection += new Vector3(transform.forward.x * verticalInput, 0f, transform.forward.z * verticalInput);
            }
            
            if (rotationDirection.magnitude > 0.01f) 
            {
                Quaternion targetRotation = Quaternion.LookRotation(rotationDirection);

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            // Halt horizontal velocity when stopped
            if (!isSittingToggled && !isMunchingHeld)
            {
                characterRigidbody.linearVelocity = new Vector3(0f, characterRigidbody.linearVelocity.y, 0f);
            }
        }
        
        characterAnimator.SetBool("IsWalking", isMoving);
    }
    
    void HandleAnimationInput()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isNearChair && !isSittingToggled)
            {
                isSittingToggled = true;
                AlignChairAndCharacter();
            }
            else if (isSittingToggled)
            {
                isSittingToggled = false;
                RestoreChairPosition();
            }
        }
        
        bool isMunchingHeld = isSittingToggled && Input.GetKey(KeyCode.E);
        
        characterAnimator.SetBool("IsMunching", isMunchingHeld);
        bool shouldBeSitting = isSittingToggled && !isMunchingHeld;
        characterAnimator.SetBool("IsSitting", shouldBeSitting);
        
        SetAnimationBool(KeyCode.Q, "IsPicking");
    }
    
    void SetAnimationBool(KeyCode key, string animatorParameterName)
    {
        bool keyPressed = Input.GetKey(key);
        characterAnimator.SetBool(animatorParameterName, keyPressed);
    }

    void HandleCameraSwitch()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (thirdPovCamera != null && firstPovCamera != null)
            {
                bool isThirdPersonActive = thirdPovCamera.activeSelf;

                thirdPovCamera.SetActive(!isThirdPersonActive);
                firstPovCamera.SetActive(isThirdPersonActive);
            }
        }
    }

    // --- Collision and Trigger Functions ---

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.StartsWith("Chair "))
        {
            isNearChair = true;
            nearestChair = other.gameObject;
            
            originalChairPosition = nearestChair.transform.position;
            originalChairRotation = nearestChair.transform.rotation;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == nearestChair)
        {
            isNearChair = false;
            
            if (isSittingToggled)
            {
                isSittingToggled = false;
                RestoreChairPosition();
            }
            
            nearestChair = null;
        }
    }
    
    // NEW: Logic to push character away from the PC Table
    void OnCollisionStay(Collision collision)
    {
        // Check for the specific object that the character should be pushed away from
        if (collision.gameObject.name.Contains("PC Table"))
        {
            // Calculate the vector pointing from the collision point outward to the character center
            Vector3 contactPoint = collision.GetContact(0).point;
            Vector3 pushDirection = transform.position - contactPoint;
            
            // Ignore the Y axis so the push doesn't affect jumping/gravity
            pushDirection.y = 0;
            pushDirection.Normalize();

            // Define the force magnitude (you may need to tweak 50f based on your character's mass)
            float pushForce = 50f; 
            
            // Apply the force immediately. This is the pushback logic.
            characterRigidbody.AddForce(pushDirection * pushForce * Time.deltaTime, ForceMode.VelocityChange);
        }
    }

    // --- Chair Logic ---

    private void AlignChairAndCharacter()
    {
        if (isSittingToggled && nearestChair != null)
        {
            float chairOriginalX = originalChairPosition.x; 
            float chairOriginalZ = originalChairPosition.z;

            bool isFacingPositiveX = transform.forward.x > 0;
            float xOffset = isFacingPositiveX ? -10f : 10f;

            Vector3 newCharacterPosition = transform.position;

            newCharacterPosition.z = chairOriginalZ;
            newCharacterPosition.x = chairOriginalX + xOffset;
            
            transform.position = new Vector3(
                newCharacterPosition.x, 
                transform.position.y, 
                newCharacterPosition.z
            );
            
            Vector3 targetChairPosition = nearestChair.transform.position;
            
            targetChairPosition.x = transform.position.x;
            targetChairPosition.z = transform.position.z;

            nearestChair.transform.position = targetChairPosition;
        }
    }
    
    private void RestoreChairPosition()
    {
        if (nearestChair != null)
        {
            nearestChair.transform.position = originalChairPosition;
            nearestChair.transform.rotation = originalChairRotation;

            nearestChair = null;
        }
    }
}