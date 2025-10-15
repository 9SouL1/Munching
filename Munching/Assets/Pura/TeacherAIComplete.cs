using UnityEngine;
using TMPro;

public class TeacherAIComplete : MonoBehaviour
{
    [Header("Player Detection Settings")]
    public Transform player;
    public float detectionRange = 10f;
    public float detectionAngle = 90f;
    public LayerMask obstructionMask;

    [Header("Movement Settings")]
    public float roamSpeed = 1.5f;
    public float chaseSpeed = 4f;
    public float roamDistance = 5f;
    public float rotationSpeed = 3f;
    public float obstacleCheckDistance = 1f;
    public LayerMask obstacleMask;

    [Header("Room Bounds Object")]
    public BoxCollider roomBounds; // <— drag your room or floor collider here!

    [Header("UI Text")]
    public TextMeshProUGUI warningText;

    private Vector3 targetPosition;
    private float caughtTimer = 0f;
    private bool playerDetected = false;
    private bool caught = false;

    void Start()
    {
        SetNewRoamTarget();
        if (warningText) warningText.text = "";
    }

    void Update()
    {
        if (!player || caught) return;

        DetectPlayer();

        if (playerDetected)
            ChasePlayer();
        else
            Roam();
    }

    // -------------------- PLAYER DETECTION --------------------
    void DetectPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer < detectionAngle / 2 && distanceToPlayer <= detectionRange)
        {
            if (!Physics.Linecast(transform.position, player.position, obstructionMask))
            {
                playerDetected = true;
                if (warningText) warningText.text = "⚠️ Player Detected!";

                caughtTimer += Time.deltaTime;
                if (caughtTimer >= 5f)
                {
                    caught = true;
                    if (warningText) warningText.text = "❌ Caught!";
                }
                return;
            }
        }

        playerDetected = false;
        caughtTimer = 0f;
        if (warningText) warningText.text = "";
    }

    // -------------------- CHASE BEHAVIOR --------------------
    void ChasePlayer()
    {
        MoveTowards(player.position, chaseSpeed);
    }

    // -------------------- ROAM BEHAVIOR --------------------
    void Roam()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget < 0.5f || IsObstacleAhead())
            SetNewRoamTarget();

        MoveTowards(targetPosition, roamSpeed);
    }

    void MoveTowards(Vector3 destination, float speed)
    {
        Vector3 direction = (destination - transform.position).normalized;
        direction.y = 0f;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);

            float angle = Quaternion.Angle(transform.rotation, targetRot);
            if (angle < 45f)
                transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    // -------------------- ROOM-BASED TARGET SELECTION --------------------
    void SetNewRoamTarget()
    {
        if (!roomBounds)
        {
            Debug.LogWarning("No roomBounds assigned to TeacherAI!");
            targetPosition = transform.position;
            return;
        }

        Bounds bounds = roomBounds.bounds;

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                transform.position.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (!Physics.Linecast(transform.position, randomPoint, obstacleMask))
            {
                targetPosition = randomPoint;
                return;
            }
        }

        targetPosition = transform.position;
    }

    bool IsObstacleAhead()
    {
        return Physics.Raycast(transform.position, transform.forward, obstacleCheckDistance, obstacleMask);
    }

    void OnDrawGizmosSelected()
    {
        if (roomBounds)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(roomBounds.bounds.center, roomBounds.bounds.size);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Vector3 leftBoundary = Quaternion.Euler(0, -detectionAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, detectionAngle / 2, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectionRange);
    }
}
