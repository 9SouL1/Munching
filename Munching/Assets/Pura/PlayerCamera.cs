using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform target; // drag CameraPivot (child of Player)
    public float distance = 3.5f; // further back for 3rd person
    public float height = 1.6f;   // camera height from player
    public float sensitivity = 2f;
    public float smoothTime = 0.08f;
    public bool firstPerson = false;

    private float yaw;
    private float pitch;
    private Vector3 velocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -40f, 80f);

        if (Input.GetKeyDown(KeyCode.V))
            firstPerson = !firstPerson;
    }

    void LateUpdate()
    {
        if (!target) return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot = target.position + Vector3.up * height;

        // Adjust distance depending on POV
        float currentDistance = firstPerson ? 0f : distance;
        Vector3 desiredPosition = pivot - rotation * Vector3.forward * currentDistance;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.rotation = rotation;
    }
}
