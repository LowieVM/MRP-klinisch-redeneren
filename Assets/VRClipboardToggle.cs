using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using Pulse.Unity;
using static XRConversationTrigger;

public class VRClipboardToggle : MonoBehaviour
{
    [Header("Clipboard Settings")]
    public Transform clipboardTransform;
    public Vector3 hiddenPosition = new Vector3(0, -500, 0);
    public Vector3 visiblePosition = new Vector3(0, -100, 0);
    public float slideSpeed = 3f;

    [Header("UI Elements")]
    public TextMeshProUGUI npcNameText;

    [Header("NPC Data")]
    public PulseEngineDriver pulseEngineDriver;

    [Header("Controller Input")]
    public InputActionReference toggleAction;

    private bool isVisible = false;
    private Vector3 targetPosition;

    private void Start()
    {
        if (clipboardTransform == null)
        {
            clipboardTransform = transform;
        }

        targetPosition = hiddenPosition;
        clipboardTransform.localPosition = hiddenPosition;

        LoadNPCData();
    }

    private void OnEnable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.Enable();
            toggleAction.action.performed += OnTogglePressed;
        }
    }

    private void OnDisable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.performed -= OnTogglePressed;
            toggleAction.action.Disable();
        }
    }

    private void Update()
    {
        clipboardTransform.localPosition = Vector3.Lerp(
            clipboardTransform.localPosition,
            targetPosition,
            Time.deltaTime * slideSpeed
        );
    }

    private void OnTogglePressed(InputAction.CallbackContext context)
    {
        ToggleClipboard();
    }

    private void ToggleClipboard()
    {
        isVisible = !isVisible;
        targetPosition = isVisible ? visiblePosition : hiddenPosition;

        Debug.Log($"Clipboard toggled: {(isVisible ? "Visible" : "Hidden")}");
    }

    private void LoadNPCData()
    {
        if (pulseEngineDriver == null)
        {
            Debug.LogWarning("PulseEngineDriver not assigned! Looking for it...");
            pulseEngineDriver = FindObjectOfType<PulseEngineDriver>();
        }

        if (pulseEngineDriver != null)
        {
            try
            {
                var fieldType = pulseEngineDriver.GetType();
                var fields = fieldType.GetFields(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(TextAsset))
                    {
                        TextAsset foundJson = field.GetValue(pulseEngineDriver) as TextAsset;
                        if (foundJson != null)
                        {
                            NPCData npcData = JsonUtility.FromJson<NPCData>(foundJson.text);

                            if (npcData != null && npcData.CurrentPatient != null)
                            {
                                UpdateClipboardInfo(npcData.CurrentPatient);
                                Debug.Log($"✓ Clipboard loaded NPC data for: {npcData.CurrentPatient.Name}");
                                return;
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load NPC data: {e.Message}");
            }
        }

        if (npcNameText != null)
        {
            npcNameText.text = "Unknown Patient";
        }
    }

    private void UpdateClipboardInfo(CurrentPatient patient)
    {
        if (npcNameText != null)
        {
            string displayText = $"Patient: {patient.Name}\n";
            displayText += $"Sex: {patient.Sex}\n";

            if (patient.Age != null && patient.Age.ScalarTime != null)
            {
                displayText += $"Age: {Mathf.RoundToInt(patient.Age.ScalarTime.Value)} {patient.Age.ScalarTime.Unit}\n";
            }

            if (patient.Weight != null && patient.Weight.ScalarMass != null)
            {
                displayText += $"Weight: {Mathf.RoundToInt(patient.Weight.ScalarMass.Value)} {patient.Weight.ScalarMass.Unit}\n";
            }

            if (patient.Height != null && patient.Height.ScalarLength != null)
            {
                displayText += $"Height: {Mathf.RoundToInt(patient.Height.ScalarLength.Value)} {patient.Height.ScalarLength.Unit}";
            }

            npcNameText.text = displayText;
        }
    }

    public void ShowClipboard()
    {
        isVisible = true;
        targetPosition = visiblePosition;
    }

    public void HideClipboard()
    {
        isVisible = false;
        targetPosition = hiddenPosition;
    }
}