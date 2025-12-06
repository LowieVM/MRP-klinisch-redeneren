using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class XRConversationTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject conversationPanel;
    public TextMeshProUGUI conversationText;
    public string conversationMessage = "Hello, welcome to the game!";

    [Header("Settings")]
    public float lettersPerSecond = 20f;
    public bool closeOnSecondInteraction = true;

    private Coroutine typingCoroutine;
    private XRSimpleInteractable interactable;
    private bool isConversationActive = false;

    private void Start()
    {
        Debug.Log("=== XRConversationTrigger Start on " + gameObject.name + " ===");

        if (conversationPanel != null)
            conversationPanel.SetActive(false);

        interactable = GetComponent<XRSimpleInteractable>();

        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnInteract);
            Debug.Log("✓ XR Interaction listener added successfully!");
        }
        else
        {
            Debug.LogError("✗ XRSimpleInteractable component NOT FOUND on " + gameObject.name);
        }
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnInteract);
        }
    }

    private void OnInteract(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        Debug.Log("!!! INTERACTION DETECTED !!! From: " + args.interactorObject.transform.name);

        if (isConversationActive && closeOnSecondInteraction)
        {
            Debug.Log("Closing conversation...");
            CloseConversation();
        }
        else
        {
            Debug.Log("Starting conversation...");
            StartConversation();
        }
    }

    private void StartConversation()
    {
        Debug.Log("StartConversation called");

        if (conversationPanel != null && conversationText != null)
        {
            conversationPanel.SetActive(true);
            isConversationActive = true;
            Debug.Log("✓ Conversation panel activated!");

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeText(conversationMessage));
        }
        else
        {
            Debug.LogError("✗ Conversation panel or text is null!");
        }
    }

    private void CloseConversation()
    {
        if (conversationPanel != null)
            conversationPanel.SetActive(false);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isConversationActive = false;
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