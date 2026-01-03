using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class ConvoMovementDisable : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The conversation UI panel")]
    public GameObject conversationPanel;

    [Tooltip("XR Move Provider (Continuous or Teleport)")]
    public LocomotionProvider locomotionProvider;
    private void Update()
    {
        if (conversationPanel == null || locomotionProvider == null)
            return;

        // Disable movement while panel is active
        locomotionProvider.enabled = !conversationPanel.activeSelf;
    }
}
