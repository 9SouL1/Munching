using UnityEngine;
using TMPro;

public class FoodPickup : MonoBehaviour
{
    [Header("UI Prompt")]
    public TextMeshProUGUI interactionText;
    public string promptMessage = "Press F to grab food";

    private bool playerNearby = false;
    private StudentController nearbyStudent;

    void Start()
    {
        if (interactionText)
            interactionText.text = "";
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.F) && nearbyStudent != null)
        {
            GrabFood(nearbyStudent);
        }
    }

    void GrabFood(StudentController student)
    {
        if (interactionText) interactionText.text = "";
        student.PickUpFood();
        Debug.Log("Food grabbed by player");
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            nearbyStudent = other.GetComponent<StudentController>();
            if (nearbyStudent != null)
            {
                playerNearby = true;
                if (interactionText) interactionText.text = promptMessage;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            nearbyStudent = null;
            if (interactionText) interactionText.text = "";
        }
    }
}
