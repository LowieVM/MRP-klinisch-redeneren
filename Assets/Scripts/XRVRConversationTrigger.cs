using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Pulse.Unity;

public class XRConversationTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject conversationPanel;
    public TextMeshProUGUI conversationText;

    [Header("NPC Data")]
    public PulseEngineDriver pulseEngineDriver;

    [Header("Conversation Templates")]
    public List<string> conversationTemplates = new List<string>
    {
        "Hi, I'm {name}. I'm feeling a bit off today.",
        "Hello! My name is {name}, I'm {age} years old.",
        "Nice to meet you. I'm {name}, and I weigh about {weight}.",
        "Hey there! {name} here. I'm {height} tall.",
        "Greetings! I'm {name}, a {age} year old {sex}.",
        "Oh hello! My name is {name}. How can I help you?",
        "Hi! I'm {name}. This place is interesting, isn't it?",
        "Welcome! I'm {name}, {age} years young!",
        "Good to see you! {name}'s the name.",
        "Hello friend! You can call me {name}."
    };

    [Header("Settings")]
    public float lettersPerSecond = 20f;
    public bool closeOnSecondInteraction = true;

    private Coroutine typingCoroutine;
    private XRSimpleInteractable interactable;
    private bool isConversationActive = false;
    private NPCData cachedNPCData;

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

        LoadNPCData();
    }

    private void LoadNPCData()
    {
        if (pulseEngineDriver == null)
        {
            Debug.LogWarning("PulseEngineDriver reference not set! Looking in nearby objects...");
            pulseEngineDriver = GetComponentInParent<PulseEngineDriver>();
            if (pulseEngineDriver == null)
            {
                pulseEngineDriver = FindObjectOfType<PulseEngineDriver>();
            }
        }

        if (pulseEngineDriver != null)
        {
            try
            {
                var fieldType = pulseEngineDriver.GetType();
                Debug.Log("=== Searching for TextAsset fields in PulseEngineDriver ===");

                var fields = fieldType.GetFields(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                foreach (var field in fields)
                {
                    Debug.Log($"Found field: {field.Name} (Type: {field.FieldType.Name})");

                    if (field.FieldType == typeof(TextAsset))
                    {
                        TextAsset foundJson = field.GetValue(pulseEngineDriver) as TextAsset;
                        if (foundJson != null)
                        {
                            Debug.Log($"✓✓✓ SUCCESS! Found TextAsset field: '{field.Name}' containing '{foundJson.name}'");
                            cachedNPCData = JsonUtility.FromJson<NPCData>(foundJson.text);
                            Debug.Log("✓✓✓ NPC Data loaded successfully for: " + cachedNPCData.CurrentPatient.Name);
                            return;
                        }
                        else
                        {
                            Debug.Log($"Field '{field.Name}' is TextAsset type but value is null");
                        }
                    }
                }

                Debug.LogError("✗✗✗ NO TextAsset field found or all TextAsset fields were null!");
            }
            catch (System.Exception e)
            {
                Debug.LogError("✗ Exception while loading NPC data: " + e.Message);
                Debug.LogError("Stack trace: " + e.StackTrace);
            }
        }
        else
        {
            Debug.LogError("✗✗✗ PulseEngineDriver is NULL! Drag the PulseEngineDriver object into the Inspector!");
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

            string message = GetRandomConversationMessage();

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeText(message));
        }
        else
        {
            Debug.LogError("✗ Conversation panel or text is null!");
        }
    }

    private string GetRandomConversationMessage()
    {
        if (conversationTemplates.Count == 0)
        {
            return "Hello there!";
        }

        string template = conversationTemplates[Random.Range(0, conversationTemplates.Count)];

        if (cachedNPCData != null && cachedNPCData.CurrentPatient != null)
        {
            var patient = cachedNPCData.CurrentPatient;

            template = template.Replace("{name}", patient.Name ?? "Unknown");
            template = template.Replace("{sex}", patient.Sex ?? "Unknown");

            if (patient.Age != null && patient.Age.ScalarTime != null)
            {
                template = template.Replace("{age}", Mathf.RoundToInt(patient.Age.ScalarTime.Value).ToString());
            }

            if (patient.Weight != null && patient.Weight.ScalarMass != null)
            {
                template = template.Replace("{weight}",
                    Mathf.RoundToInt(patient.Weight.ScalarMass.Value) + " " + patient.Weight.ScalarMass.Unit);
            }

            if (patient.Height != null && patient.Height.ScalarLength != null)
            {
                template = template.Replace("{height}",
                    Mathf.RoundToInt(patient.Height.ScalarLength.Value) + " " + patient.Height.ScalarLength.Unit);
            }
        }
        else
        {
            template = template.Replace("{name}", "the NPC");
            template = template.Replace("{age}", "??");
            template = template.Replace("{sex}", "unknown");
            template = template.Replace("{weight}", "unknown");
            template = template.Replace("{height}", "unknown");
        }

        return template;
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