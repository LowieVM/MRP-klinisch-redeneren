using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections;
using Pulse.Unity;

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
    public XRNode controllerHand = XRNode.LeftHand;
    public InputHelpers.Button toggleButton = InputHelpers.Button.SecondaryButton;

    private bool isVisible = false;
    private Vector3 targetPosition;
    private bool buttonWasPressed = false;

    [System.Serializable]
    public class NPCData
    {
        public CurrentPatient CurrentPatient;
    }

    [System.Serializable]
    public class CurrentPatient
    {
        public string Name;
        public string Sex;
        public AgeData Age;
        public WeightData Weight;
        public HeightData Height;
    }

    [System.Serializable]
    public class AgeData
    {
        public ScalarTime ScalarTime;
    }

    [System.Serializable]
    public class WeightData
    {
        public ScalarMass ScalarMass;
    }

    [System.Serializable]
    public class HeightData
    {
        public ScalarLength ScalarLength;
    }

    [System.Serializable]
    public class ScalarTime
    {
        public float Value;
        public string Unit;
    }

    [System.Serializable]
    public class ScalarMass
    {
        public float Value;
        public string Unit;
    }

    [System.Serializable]
    public class ScalarLength
    {
        public float Value;
        public string Unit;
    }

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

    private void Update()
    {
        CheckToggleInput();

        clipboardTransform.localPosition = Vector3.Lerp(
            clipboardTransform.localPosition,
            targetPosition,
            Time.deltaTime * slideSpeed
        );
    }

    private void CheckToggleInput()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerHand);

        if (device.isValid)
        {
            bool buttonPressed;
            if (device.TryGetFeatureValue(new InputFeatureUsage<bool>(toggleButton.ToString()), out buttonPressed))
            {
                if (buttonPressed && !buttonWasPressed)
                {
                    ToggleClipboard();
                }
                buttonWasPressed = buttonPressed;
            }
        }
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

        // Fallback
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