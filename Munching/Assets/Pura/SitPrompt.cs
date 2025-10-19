using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SitPrompt : MonoBehaviour
{
    public Transform seatPoint;
    private bool isPlayerNear = false;
    private Transform player;
    private bool isSitting = false;

    private void Start()
    {
        // Auto-create SeatPoint if missing
        if (seatPoint == null)
        {
            GameObject sp = new GameObject("SeatPoint");
            sp.transform.SetParent(transform);
            sp.transform.localPosition = new Vector3(0, 0.6f, 0);
            seatPoint = sp.transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            player = other.transform;
            SitPromptManager.Instance?.ShowPrompt("Press C to Sit");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            SitPromptManager.Instance?.HidePrompt();
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
        SitPromptManager.Instance?.ShowPrompt("Press C to Stand");

        player.position = seatPoint.position;
        player.rotation = seatPoint.rotation;

        var controller = player.GetComponent<StudentController>();
        if (controller != null) controller.enabled = false;
    }

    void Stand()
    {
        isSitting = false;
        SitPromptManager.Instance?.ShowPrompt("Press C to Sit");

        var controller = player.GetComponent<StudentController>();
        if (controller != null) controller.enabled = true;

        player.position += player.forward * 0.5f;
    }
}
