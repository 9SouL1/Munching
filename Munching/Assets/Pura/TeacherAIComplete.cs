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
    public float rotationSpeed = 2f;

    [Header("Room Bounds Object")]
    public BoxCollider roomBounds; // drag your room collider here!

    [Header("UI Text")]
    public TextMeshProUGUI teacherStateText;

    private Vector3 roamTarget;
    private bool playerDetected = false;
    private float idleTimer = 0f;
    private float idleDuration = 0f;

    private enum TeacherState { Roaming, Chasing, Idle }
    private TeacherState currentState = TeacherState.Roaming;

    // reference to student controller
    private StudentController studentController;

    void Start()
    {
        if (!roomBounds)
            Debug.LogWarning("No roomBounds assigned to TeacherAI!");

        if (player != null)
            studentController = player.GetComponent<StudentController>();

        PickNewRoamTarget();
    }

    void Update()
    {
        if (studentController == null && player != null)
            studentController = player.GetComponent<StudentController>();

        switch (currentState)
        {
            case TeacherState.Roaming:
                HandleRoaming();
                DetectPlayer();
                break;

            case TeacherState.Chasing:
                HandleChasing();
                break;

            case TeacherState.Idle:
                HandleIdle();
                DetectPlayer();
                break;
        }

        UpdateUI();
    }

    // -------------------- DETECTION --------------------
    void DetectPlayer()
    {
        if (player == null || studentController == null)
        {
            playerDetected = false;
            return;
        }

        // Stop chasing if seated (not eating)
        if (studentController.state == StudentState.Seated)
        {
            playerDetected = false;
            if (currentState == TeacherState.Chasing)
            {
                currentState = TeacherState.Roaming;
                PickNewRoamTarget();
            }
            return;
        }

        bool canBeChased = studentController.state == StudentState.WalkingOnSight ||
                           studentController.state == StudentState.SeatedAndEating;

        if (!canBeChased)
        {
            playerDetected = false;
            return;
        }

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (dist < detectionRange && angle < detectionAngle / 2f)
        {
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer, dist, obstructionMask))
            {
                playerDetected = true;
                currentState = TeacherState.Chasing;
                return;
            }
        }

        playerDetected = false;
    }

    // -------------------- ROAMING --------------------
    void HandleRoaming()
    {
        float distance = Vector3.Distance(transform.position, roamTarget);

        if (distance < 0.5f)
        {
            currentState = TeacherState.Idle;
            idleDuration = Random.Range(1f, 3f);
            idleTimer = 0f;
            return;
        }

        MoveTowards(roamTarget, roamSpeed);
    }

    void PickNewRoamTarget()
    {
        if (roomBounds == null)
        {
            roamTarget = transform.position;
            return;
        }

        Bounds b = roomBounds.bounds;

        for (int i = 0; i < 12; i++)
        {
            Vector3 p = new Vector3(
                Random.Range(b.min.x, b.max.x),
                transform.position.y,
                Random.Range(b.min.z, b.max.z)
            );

            if (!Physics.Linecast(transform.position, p, obstructionMask))
            {
                roamTarget = p;
                return;
            }
        }

        roamTarget = transform.position;
    }

    // -------------------- CHASING --------------------
    void HandleChasing()
    {
        if (player == null || studentController == null)
        {
            currentState = TeacherState.Roaming;
            PickNewRoamTarget();
            return;
        }

        if (studentController.state == StudentState.Seated)
        {
            currentState = TeacherState.Roaming;
            playerDetected = false;
            PickNewRoamTarget();
            return;
        }

        if (!(studentController.state == StudentState.WalkingOnSight ||
              studentController.state == StudentState.SeatedAndEating))
        {
            currentState = TeacherState.Roaming;
            playerDetected = false;
            PickNewRoamTarget();
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRange * 1.5f)
        {
            currentState = TeacherState.Roaming;
            playerDetected = false;
            PickNewRoamTarget();
            return;
        }

        MoveTowards(player.position, chaseSpeed);
    }

    // -------------------- IDLE --------------------
    void HandleIdle()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {
            PickNewRoamTarget();
            currentState = TeacherState.Roaming;
        }
    }

    // -------------------- MOVEMENT --------------------
    void MoveTowards(Vector3 destination, float speed)
    {
        Vector3 dir = (destination - transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    // -------------------- UI --------------------
    void UpdateUI()
    {
        if (teacherStateText == null) return;

        if (currentState == TeacherState.Chasing && studentController != null)
        {
            if (studentController.state == StudentState.WalkingOnSight)
                teacherStateText.text = "Teacher: Chase";
            else if (studentController.state == StudentState.SeatedAndEating)
                teacherStateText.text = "Teacher: Chasing (Player Eating)";
            else
                teacherStateText.text = "Teacher: Chasing";
            return;
        }

        if (studentController != null && studentController.state == StudentState.Seated)
        {
            teacherStateText.text = "Teacher: Stop Chasing";
            return;
        }

        if (currentState == TeacherState.Roaming)
            teacherStateText.text = "Teacher: Roaming";
        else
            teacherStateText.text = "Teacher: Idle";
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
    }
}
