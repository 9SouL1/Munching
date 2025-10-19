using UnityEngine;
using TMPro;

public class SitPrompt : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI interactText;
    public string promptMessage = "Press C to sit";

    [Header("Seat Settings")]
    public Transform seatPoint; // assign a child transform on the chair where the player should sit

    private bool playerNearby = false;
    private Transform playerRoot;
    private Rigidbody playerRb;
    private PlayerMovement playerMovement;
    private StudentController studentController;

    void Start()
    {
        if (interactText != null)
            interactText.text = "";
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.C))
        {
            if (studentController != null && playerRoot != null && seatPoint != null)
            {
                SitPlayer();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            playerRoot = other.transform;
            playerRb = other.GetComponent<Rigidbody>();
            playerMovement = other.GetComponent<PlayerMovement>();
            studentController = other.GetComponent<StudentController>();

            if (interactText != null)
                interactText.text = promptMessage;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            playerRoot = null;
            playerRb = null;
            playerMovement = null;
            studentController = null;

            if (interactText != null)
                interactText.text = "";
        }
    }

    void SitPlayer()
    {
        // change player state
        studentController.SitDown();
        studentController.ignoreNextInput = true; // prevents immediate toggle back

        // move player to seat position & rotation
        playerRoot.position = seatPoint.position;
        playerRoot.rotation = seatPoint.rotation;

        // stop physics movement
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.isKinematic = true;
        }

        // disable player movement temporarily
        if (playerMovement != null)
            playerMovement.enabled = false;

        // parent player to chair
        playerRoot.SetParent(seatPoint);

        // clear prompt text
        if (interactText != null)
            interactText.text = "";
    }

    // optional if you want to stand them up externally
    public void ForceStandPlayer()
    {
        if (playerRoot == null || studentController == null) return;

        studentController.StandUp();

        if (playerRb != null)
            playerRb.isKinematic = false;

        if (playerMovement != null)
            playerMovement.enabled = true;

        playerRoot.SetParent(null);
    }
}
