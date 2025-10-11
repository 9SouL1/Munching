using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    Rigidbody rb;
    Vector2 input;
    bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        // read input (no physics here)
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        // apply movement in physics step
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        Vector3 target = rb.position + move.normalized * moveSpeed * Time.fixedDeltaTime;
        // move using MovePosition for smooth physics-friendly movement
        rb.MovePosition(target);
    }

    void OnCollisionStay(Collision c)
    {
        if (c.gameObject.CompareTag("Ground")) isGrounded = true;
    }

    void OnCollisionExit(Collision c)
    {
        if (c.gameObject.CompareTag("Ground")) isGrounded = false;
    }
}
