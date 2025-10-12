using UnityEngine;

public class TeacherPatrol : MonoBehaviour
{
    public Transform[] waypoints;  // assign waypoints in Inspector
    public float moveSpeed = 3f;
    public float stopDistance = 0.5f;

    private int currentIndex = 0;

    void Update()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        Vector3 direction = (target.position - transform.position).normalized;
        Vector3 move = direction * moveSpeed * Time.deltaTime;

        transform.position += move;
        transform.LookAt(target);

        // Check if near the target point
        if (Vector3.Distance(transform.position, target.position) < stopDistance)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length; // loop
        }
    }
}
