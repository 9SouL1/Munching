using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public Transform target; // usually CameraPivot (child of Player)
    private StudentController studentController;

    [Header("General Settings")]
    public float sensitivity = 2f;
    public float smoothTime = 0.08f;

    [Header("View Settings")]
    public float thirdPersonDistance = 3.5f;
    public float firstPersonDistance = 0f;
    public float standingHeight = 1.6f;
    public float seatedHeight = 1.0f;

    private float yaw;
    private float pitch;
    private Vector3 velocity;

    private bool isThirdPerson = true;
    private bool wasSeated = false; // track previous state to detect changes

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (target != null)
            studentController = target.GetComponentInParent<StudentController>();
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -60f, 80f);

        // Toggle between first and third person (like Minecraft)
        if (Input.GetKeyDown(KeyCode.V))
            isThirdPerson = !isThirdPerson;
    }

    void LateUpdate()
    {
        if (!target) return;

        bool isSeated = false;
        if (studentController != null)
        {
            isSeated = studentController.state == StudentState.Seated ||
                       studentController.state == StudentState.SeatedAndEating;
        }

        // ?? Detect change (Standing <-> Sitting)
        if (isSeated != wasSeated)
        {
            if (isSeated)
                Debug.Log("?? Player is now SEATED.");
            else
                Debug.Log("?? Player is now STANDING.");

            wasSeated = isSeated;
        }

        float currentHeight = isSeated ? seatedHeight : standingHeight;
        float currentDistance = isThirdPerson ? thirdPersonDistance : firstPersonDistance;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot = target.position + Vector3.up * currentHeight;

        Vector3 desiredPosition = pivot - rotation * Vector3.forward * currentDistance;

        // Smooth transition
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.rotation = rotation;
    }
}
