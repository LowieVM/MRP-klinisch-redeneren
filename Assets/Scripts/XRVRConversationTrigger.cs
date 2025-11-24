using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class XRConversationTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject conversationPanel;
    public TextMeshProUGUI conversationText;
    public string conversationMessage = "Hello, welcome to the game!";

[Header("Settings")]
    public float lettersPerSecond = 20f; // Speed of text typing  

    private Coroutine typingCoroutine;

    private void Start()
    {
        if (conversationPanel != null)
            conversationPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartConversation();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (conversationPanel != null && conversationPanel.activeSelf)
                conversationPanel.SetActive(false);

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
        }
    }

    private void StartConversation()
    {
        if (conversationPanel != null && conversationText != null)
        {
            conversationPanel.SetActive(true);

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeText(conversationMessage));
        }
    }

    private IEnumerator TypeText(string message)
    {
        conversationText.text = "";
        foreach (char letter in message)
        {
            conversationText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
    }  
}
