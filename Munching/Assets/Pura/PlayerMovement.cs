using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float runSpeed = 11f;
    public float jumpForce = 5f;
    public Transform cameraTransform;

    private Rigidbody rb;
    private bool isGrounded;
    private StudentController studentController;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        studentController = GetComponent<StudentController>();

        if (studentController == null)
            Debug.LogWarning("StudentController not found on Player. Add StudentController to use sit/eat states.");

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleSitAndEatInput();

        // Stop movement if seated
        if (studentController != null &&
            (studentController.state == StudentState.Seated || studentController.state == StudentState.SeatedAndEating))
            return;

        HandleMovement();
    }

    void HandleSitAndEatInput()
    {
        if (studentController == null) return;

        // Skip input for one frame if SitPrompt triggered sit
        if (studentController.ignoreNextInput)
        {
            studentController.ignoreNextInput = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (studentController.state == StudentState.Seated || studentController.state == StudentState.SeatedAndEating)
            {
                studentController.StandUp();
                rb.isKinematic = false;
                transform.SetParent(null);
                enabled = true;
            }
            else
            {
                studentController.SitDown();
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (studentController.state == StudentState.Seated && studentController.hasFood)
            {
                studentController.StartEating();
            }
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * moveZ + right * moveX).normalized;

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        Vector3 targetVel = moveDir * speed;
        rb.linearVelocity = new Vector3(targetVel.x, rb.linearVelocity.y, targetVel.z);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            studentController?.SetWalkingOnSight();
        }
        else
        {
            studentController?.SetIdle();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
