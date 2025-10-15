using UnityEngine;
using TMPro;

public class FoodPickup : MonoBehaviour
{
    [Header("UI Prompt")]
    public TextMeshProUGUI interactionText;  // Drag your TMP text here
    public string promptMessage = "Press F to grab food";

    private bool isPlayerNearby = false;

    void Start()
    {
        if (interactionText)
            interactionText.text = "";
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            GrabFood();
        }
    }

    void GrabFood()
    {
        if (interactionText)
            interactionText.text = "";

        // You can change this depending on what should happen when food is grabbed:
        // - Play animation
        // - Add score
        // - Hide or destroy the food
        Debug.Log("Food grabbed!");
        Destroy(gameObject); // remove the food
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactionText)
                interactionText.text = promptMessage;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactionText)
                interactionText.text = "";
        }
    }
}
