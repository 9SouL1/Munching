using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform target; // drag CameraPivot (child of Player) here
    public float distance = 4f;
    public float height = 1.6f;
    public float sensitivity = 2f;
    public float smoothTime = 0.06f;
    public bool firstPerson = false;

    float yaw, pitch;
    Vector3 velocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // read mouse in Update and accumulate
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, -40f, 80f);

        if (Input.GetKeyDown(KeyCode.V)) firstPerson = !firstPerson;
    }

    void FixedUpdate()
    {
        if (!target) return;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot = target.position + Vector3.up * height;
        Vector3 desired = firstPerson ? pivot : pivot - rot * Vector3.forward * distance;

        // Smooth using SmoothDamp with FixedUpdate timestep
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        transform.rotation = rot;
    }
}
