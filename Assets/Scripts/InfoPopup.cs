using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using static XRConversationTrigger;

public class InfoPopup : MonoBehaviour
{
    public GameObject infoCanvas;
    public TextMeshProUGUI infoText;
    public string infoMessage = "Default info message.";

    public float lettersPerSecond = 20f;
    public bool closeOnSecondInteraction = true;

    private Coroutine typingCoroutine;
    private XRSimpleInteractable interactable;
    private bool isConversationActive = false;

    void Start()
    {
        if (infoCanvas != null)
            infoCanvas.SetActive(false);

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

        if (infoCanvas != null && infoCanvas != null)
        {
            infoCanvas.SetActive(true);
            isConversationActive = true;
            Debug.Log("✓ Conversation panel activated!");

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeText(infoMessage));
        }
        else
        {
            Debug.LogError("✗ Conversation panel or text is null!");
        }
    }

    private IEnumerator TypeText(string message)
    {
        infoText.text = "";

        foreach (char letter in message)
        {
            infoText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
    }

    private void CloseConversation()
    {
        if (infoCanvas != null)
            infoCanvas.SetActive(false);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isConversationActive = false;
    }
}
