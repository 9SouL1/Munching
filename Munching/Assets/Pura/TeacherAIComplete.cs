using UnityEngine;
using TMPro;

public class TeacherAIComplete : MonoBehaviour
{
    [Header("References")]
    public Transform player;           // assign player capsule here
    public TMP_Text warningText;       // assign TextMeshPro UI text here
    public BoxCollider roomBounds;     // assign BoxCollider that covers your room

    [Header("Movement Settings")]
    public float roamSpeed = 2f;
    public float chaseSpeed = 6f;
    public float roamDistance = 5f;           // max distance for random roam step
    public float obstacleCheckDistance = 1f;  // how far ahead to raycast for obstacles
    public LayerMask obstacleMask;            // set this to the Obstacle layer(s)

    [Header("Detection Settings")]
    public float detectionRange = 10f;
    public float detectionAngle = 90f;
    public LayerMask obstructionMask;         // layers that block LOS (walls, etc.)

    [Header("Caught Settings")]
    public float caughtTime = 5f;             // seconds to be caught

    private Vector3 roamTarget;
    private bool playerDetected = false;
    private float detectionTimer = 0f;

    void Start()
    {
        if (roomBounds == null) Debug.LogWarning("RoomBounds not assigned! Teacher may roam outside.");
        SetNewRoamTarget();
    }

    void Update()
    {
        DetectPlayer();
        HandleDetection();
        MoveTeacher();
    }

    // ---------- Detection ----------
    void DetectPlayer()
    {
        if (!player) return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle < detectionAngle / 2 && distance <= detectionRange)
        {
            // Line-of-sight check (true = blocked)
            if (!Physics.Linecast(transform.position, player.position, obstructionMask))
            {
                playerDetected = true;
                return;
            }
        }

        playerDetected = false;
    }

    void HandleDetection()
    {
        if (playerDetected)
        {
            detectionTimer += Time.deltaTime;
            if (warningText != null) warningText.text = "Player Detected!";

            if (detectionTimer >= caughtTime)
            {
                if (warningText != null) warningText.text = "Caught!";
                Debug.Log("Player Caught!");
                // optional: add reset / game over logic here
            }
        }
        else
        {
            detectionTimer = 0f;
            if (warningText != null) warningText.text = "";
        }
    }

    // ---------- Movement ----------
    void MoveTeacher()
    {
        if (playerDetected)
        {
            // Chase player (only moves if no obstacle directly blocking immediate path)
            Vector3 dir = (player.position - transform.position).normalized;
            bool blocked = Physics.Raycast(transform.position, dir, obstacleCheckDistance, obstacleMask);
            if (!blocked)
            {
                MoveTowards(player.position, chaseSpeed);
            }
            else
            {
                // optional: choose a nearby point to navigate around; for simplicity, pick new roam target
                SetNewRoamTarget();
            }
        }
        else
        {
            // Roam toward roamTarget, avoid obstacles
            Vector3 dir = (roamTarget - transform.position).normalized;
            // if path ahead blocked, pick new roam target
            if (Physics.Raycast(transform.position, dir, obstacleCheckDistance, obstacleMask))
            {
                SetNewRoamTarget();
                return;
            }

            MoveTowards(roamTarget, roamSpeed);

            if (Vector3.Distance(transform.position, roamTarget) < 0.5f)
                SetNewRoamTarget();
        }
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 dir = (target - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }

    // ---------- Roam Target ----------
    void SetNewRoamTarget()
    {
        Vector3 randomDir = new Vector3(
            Random.Range(-roamDistance, roamDistance),
            0,
            Random.Range(-roamDistance, roamDistance)
        );

        Vector3 potentialTarget = transform.position + randomDir;

        // clamp inside room bounds if assigned
        if (roomBounds != null)
        {
            Vector3 min = roomBounds.bounds.min;
            Vector3 max = roomBounds.bounds.max;

            potentialTarget.x = Mathf.Clamp(potentialTarget.x, min.x, max.x);
            potentialTarget.z = Mathf.Clamp(potentialTarget.z, min.z, max.z);
            potentialTarget.y = transform.position.y;
        }

        roamTarget = potentialTarget;
        Debug.Log("New roam target: " + roamTarget);
    }

    // ---------- Gizmos ----------
    void OnDrawGizmosSelected()
    {
        // detection FOV rays
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * detectionRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, detectionAngle / 2, 0) * transform.forward * detectionRange);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, -detectionAngle / 2, 0) * transform.forward * detectionRange);

        // room bounds
        if (roomBounds != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(roomBounds.bounds.center, roomBounds.bounds.size);
        }

        // roamDistance visual
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, roamDistance);
    }
}
