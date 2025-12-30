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

    [Header("Patient State")]
    public PatientStateManager patientStateManager;

    [Header("Critical State - Truthful")]
    public List<string> criticalTruthfulConversations = new List<string>
    {
        "I'm {name}... the pain is unbearable, please help me!",
        "My chest really hurts... I'm {age} and I've never felt like this.",
        "Please... I'm {name}, I can barely breathe...",
        "Something's very wrong... I'm in severe pain.",
        "I need help immediately! The pain is too much!"
    };

    [Header("Critical State - Lying")]
    public List<string> criticalLyingConversations = new List<string>
    {
        "I'm fine, really... just a little dizzy. I'm {name}.",
        "No need to worry about me, I just need to sit down.",
        "It's nothing serious... I'm {name}, I'll be okay.",
        "Just a headache, nothing more. I'm sure it'll pass.",
        "I feel completely fine, no problems at all."
    };

    [Header("Unstable State - Truthful")]
    public List<string> unstableTruthfulConversations = new List<string>
    {
        "I'm {name}... still hurting but the medicine is helping a bit.",
        "The pain is still there... but not as bad as before.",
        "I'm {name}, {age} years old. I'm starting to feel a little better.",
        "Thank you for helping... I still feel weak though.",
        "It's improving slowly... but I'm not out of the woods yet."
    };

    [Header("Unstable State - Lying")]
    public List<string> unstableLyingConversations = new List<string>
    {
        "I'm totally fine now! {name} here, feeling great!",
        "All better! I'm {name}, no more pain at all.",
        "I don't need any more treatment, I'm completely recovered.",
        "The pain is completely gone, I swear.",
        "I'm {name} and I feel perfect now, thanks!"
    };

    [Header("Improving State - Truthful")]
    public List<string> improvingTruthfulConversations = new List<string>
    {
        "Hi! I'm {name}. Feeling much better now, thank you.",
        "I'm {name}, the treatment really helped. Almost back to normal.",
        "Much better! I'm {age} and feeling like myself again.",
        "The pain is mostly gone now. I'm {name}, thanks for your care.",
        "I'm improving steadily. {name}'s the name, grateful for your help."
    };

    [Header("Improving State - Lying")]
    public List<string> improvingLyingConversations = new List<string>
    {
        "I'm {name}... actually still feeling some pain.",
        "Well... I'm better but not as good as I'm saying. I'm {name}.",
        "The pain isn't completely gone yet, if I'm being honest.",
        "I'm {name}. Still a bit uncomfortable but didn't want to worry you.",
        "Trying to be brave, but I still feel some symptoms."
    };

    [Header("Stable State - Truthful")]
    public List<string> stableTruthfulConversations = new List<string>
    {
        "Hi, I'm {name}. I'm feeling completely healthy now!",
        "Hello! My name is {name}, I'm {age} years old and fully recovered.",
        "Nice to meet you. I'm {name}, feeling great thanks to you!",
        "Hey there! {name} here. Back to normal, thank you so much!",
        "Greetings! I'm {name}, a {age} year old {sex}, and I feel wonderful!",
        "Oh hello! My name is {name}. I'm completely better now!",
        "Hi! I'm {name}. Everything is perfect, no more problems!",
        "Welcome! I'm {name}, {age} years young and healthy!",
        "Good to see you! {name}'s the name. Feeling fantastic!",
        "Hello friend! You can call me {name}. I'm all better!"
    };

    [Header("Stable State - Lying")]
    public List<string> stableLyingConversations = new List<string>
    {
        "I'm {name}... maybe I pushed myself too hard, feeling a bit off.",
        "Hi, I'm {name}. Probably nothing, but I'm feeling slightly unwell again.",
        "I'm {name}. Everything's fine... mostly. Just a small concern.",
        "Hello! {name} here. I might have overdone it, feeling tired.",
        "I'm completely fine! Well... maybe a tiny bit of discomfort."
    };

    [Header("Settings")]
    public float lettersPerSecond = 20f;
    public bool closeOnSecondInteraction = true;

    private Coroutine typingCoroutine;
    private XRSimpleInteractable interactable;
    private bool isConversationActive = false;
    private NPCData cachedNPCData;
    private bool lastResponseWasTruthful;
    private PatientState lastKnownState;

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

        // Subscribe to patient state events if manager exists
        if (patientStateManager != null)
        {
            patientStateManager.OnStateChanged += HandleStateChanged;
            patientStateManager.OnTruthfulnessChecked += HandleTruthfulnessChecked;
            patientStateManager.OnPatientFullyHealed += HandlePatientFullyHealed;
            lastKnownState = patientStateManager.GetCurrentState();
        }
        else
        {
            Debug.LogWarning("PatientStateManager not assigned! Conversations will use default templates.");
        }
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

        if (patientStateManager != null)
        {
            patientStateManager.OnStateChanged -= HandleStateChanged;
            patientStateManager.OnTruthfulnessChecked -= HandleTruthfulnessChecked;
            patientStateManager.OnPatientFullyHealed -= HandlePatientFullyHealed;
        }
    }

    #region Event Handlers

    private void HandleStateChanged(PatientState newState)
    {
        lastKnownState = newState;
        Debug.Log($"[Conversation] Patient state changed to: {newState}");

        // Optional: Show a visual indicator or update UI
    }

    private void HandleTruthfulnessChecked(bool isTruthful)
    {
        lastResponseWasTruthful = isTruthful;
        Debug.Log($"[Conversation] Patient is being {(isTruthful ? "TRUTHFUL" : "DECEPTIVE")}");
    }

    private void HandlePatientFullyHealed()
    {
        Debug.Log("[Conversation] Patient has been fully healed!");
        // Optional: Trigger celebration animation or dialogue
    }

    #endregion

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
        PatientState currentState = patientStateManager != null
            ? patientStateManager.GetCurrentState()
            : PatientState.Stable;

        bool isTruthful = patientStateManager != null
            ? patientStateManager.IsTruthful()
            : true;

        List<string> templates = GetConversationTemplatesForState(currentState, isTruthful);

        if (templates.Count == 0)
        {
            Debug.LogWarning($"No conversation templates for {currentState} + {(isTruthful ? "Truthful" : "Lying")}");
            return "...";
        }

        string template = templates[Random.Range(0, templates.Count)];
        return ReplaceTemplatePlaceholders(template);
    }

    private List<string> GetConversationTemplatesForState(PatientState state, bool isTruthful)
    {
        return state switch
        {
            PatientState.Critical => isTruthful ? criticalTruthfulConversations : criticalLyingConversations,
            PatientState.Unstable => isTruthful ? unstableTruthfulConversations : unstableLyingConversations,
            PatientState.Improving => isTruthful ? improvingTruthfulConversations : improvingLyingConversations,
            PatientState.Stable => isTruthful ? stableTruthfulConversations : stableLyingConversations,
            _ => stableTruthfulConversations
        };
    }

    private string ReplaceTemplatePlaceholders(string template)
    {
        if (cachedNPCData != null && cachedNPCData.CurrentPatient != null)
        {
            var patient = cachedNPCData.CurrentPatient;

            template = template.Replace("{name}", patient.Name ?? "Unknown");
            template = template.Replace("{sex}", patient.Sex ?? "Unknown");

            if (patient.Age != null && patient.Age.ScalarTime != null)
            {
                template = template.Replace("{age}", Mathf.RoundToInt(patient.Age.ScalarTime.Value).ToString());
            }
            else
            {
                template = template.Replace("{age}", "??");
            }

            if (patient.Weight != null && patient.Weight.ScalarMass != null)
            {
                template = template.Replace("{weight}",
                    Mathf.RoundToInt(patient.Weight.ScalarMass.Value) + " " + patient.Weight.ScalarMass.Unit);
            }
            else
            {
                template = template.Replace("{weight}", "unknown");
            }

            if (patient.Height != null && patient.Height.ScalarLength != null)
            {
                template = template.Replace("{height}",
                    Mathf.RoundToInt(patient.Height.ScalarLength.Value) + " " + patient.Height.ScalarLength.Unit);
            }
            else
            {
                template = template.Replace("{height}", "unknown");
            }
        }
        else
        {
            template = template.Replace("{name}", "the patient");
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

    // voor debugging
    public bool WasLastResponseTruthful()
    {
        return lastResponseWasTruthful;
    }

    public PatientState GetCurrentPatientState()
    {
        return lastKnownState;
    }

    public void TriggerConversation()
    {
        if (!isConversationActive)
        {
            StartConversation();
        }
    }
}