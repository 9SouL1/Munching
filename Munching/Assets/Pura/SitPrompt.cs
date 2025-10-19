using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class SitPrompt : MonoBehaviour
{
    public TextMeshProUGUI interactText;
    public Transform seatPoint; // can be auto-generated
    private bool isPlayerNear = false;
    private Transform player;
    private bool isSitting = false;

    private void Start()
    {
        // Auto-create a SeatPoint if missing
        if (seatPoint == null)
        {
            GameObject sp = new GameObject("SeatPoint");
            sp.transform.SetParent(transform);
            sp.transform.localPosition = new Vector3(0, 0.6f, 0); // height above chair seat
            seatPoint = sp.transform;
        }

        // Hide text initially
        if (interactText != null)
            interactText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            player = other.transform;
            if (interactText != null)
                interactText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactText != null)
                interactText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.C))
        {
            if (!isSitting)
                Sit();
            else
                Stand();
        }
    }

    void Sit()
    {
        isSitting = true;
        if (interactText != null)
            interactText.text = "Press C to stand";

        player.position = seatPoint.position;
        player.rotation = seatPoint.rotation;

        var controller = player.GetComponent<StudentController>();
        if (controller != null) controller.enabled = false;
    }

    void Stand()
    {
        isSitting = false;
        if (interactText != null)
            interactText.text = "Press C to sit";

        var controller = player.GetComponent<StudentController>();
        if (controller != null) controller.enabled = true;

        player.position += player.forward * 0.5f; // step forward
    }
}
