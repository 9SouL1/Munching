using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 6f;
    public float runSpeed = 11f;
    public float jumpForce = 5f;
    public Transform cameraTransform; // <— Reference to camera

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // --- Get movement input ---
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // --- Calculate direction based on camera forward ---
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Remove vertical tilt (so movement stays flat)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * moveZ + right * moveX).normalized;

        // --- Choose speed ---
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // --- Apply velocity ---
        Vector3 velocity = moveDir * speed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        // --- Jump ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // --- Face camera direction when moving ---
        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
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
