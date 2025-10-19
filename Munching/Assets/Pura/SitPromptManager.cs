using UnityEngine;
using TMPro;

public class SitPromptManager : MonoBehaviour
{
    public static SitPromptManager Instance;
    public TextMeshProUGUI promptText;

    private void Awake()
    {
        Instance = this;
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    public void ShowPrompt(string message)
    {
        if (promptText == null) return;

        promptText.text = message;
        promptText.gameObject.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptText == null) return;

        promptText.gameObject.SetActive(false);
    }
}
