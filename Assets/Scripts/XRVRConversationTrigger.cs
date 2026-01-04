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
    "Sweetheart... it's Mom... the pain is so bad, I need your help!",
    "Honey, I'm scared... I've never felt pain like this before.",
    "{name} here... your mother. Please help me, I can barely breathe...",
    "My dear, something's very wrong... I'm in so much pain.",
    "I need you right now, sweetie... the pain is unbearable!",
    "Baby girl, I'm trying to be brave but... this really hurts.",
    "It's your mom... I hate to worry you but I really need help."
};

    [Header("Critical State - Lying")]
    public List<string> criticalLyingConversations = new List<string>
{
    "I'm fine, honey... you know your mother, tough as nails. Just a little dizzy.",
    "Don't fuss over me, dear. I'm {name}, I've been through worse.",
    "Sweetheart, it's nothing... I don't want you to worry about your old mom.",
    "Just a headache, baby. You know how I get. It'll pass.",
    "I feel completely fine! Stop looking at me like that, I raised you better than to worry.",
    "Your mother is perfectly fine, dear. Now stop fretting over me.",
    "I'm okay, sweetie. Let's not make a big deal out of nothing."
};

    [Header("Unstable State - Truthful")]
    public List<string> unstableTruthfulConversations = new List<string>
{
    "Hi honey... Mom's still hurting but whatever you gave me is helping a bit.",
    "The pain is still there, sweetie... but not as bad as before. You're doing great.",
    "It's your mom, {age} years young. I'm starting to feel a little better, thanks to you.",
    "Thank you for taking care of your mother... I still feel weak though.",
    "You're a wonderful nurse, honey. It's improving slowly... but I'm not out of danger yet.",
    "Mom's hanging in there, baby. The treatment is working.",
    "Still not great, sweetheart, but you're helping. I'm proud of you."
};

    [Header("Unstable State - Lying")]
    public List<string> unstableLyingConversations = new List<string>
{
    "I'm totally fine now, dear! Your mom's back to normal!",
    "All better! See? {name}'s tough as always. No more pain at all.",
    "I don't need any more treatment, honey. You've done enough.",
    "The pain is completely gone, I promise. Stop worrying about me!",
    "I feel perfect now, sweetie! You can focus on other patients.",
    "Your mother is fine! I've always bounced back quickly, you know that.",
    "See? Good as new! Now stop fussing over your old mom."
};

    [Header("Improving State - Truthful")]
    public List<string> improvingTruthfulConversations = new List<string>
{
    "Hi baby! Mom's feeling much better now, you did wonderfully.",
    "It's {name}, your proud mother. The treatment really helped, almost back to normal.",
    "Much better, honey! Your {age}-year-old mom is feeling like herself again.",
    "The pain is mostly gone now. You saved your mother's life, sweetheart.",
    "I'm improving steadily thanks to you. You're an amazing nurse, and I'm not just saying that because you're my daughter.",
    "You took such good care of me, dear. Mom's on the mend.",
    "Feeling so much better! I knew my daughter would fix me up."
};

    [Header("Improving State - Lying")]
    public List<string> improvingLyingConversations = new List<string>
{
    "I'm your mom... actually still feeling some pain, honey. Sorry for fibbing.",
    "Well... Mom's better but not as good as I said. Didn't want you to worry.",
    "The pain isn't completely gone yet, if I'm being honest with my daughter.",
    "It's {name}. Still a bit uncomfortable but I didn't want to be a burden to you.",
    "Trying to be brave for you, sweetie, but I still feel some symptoms.",
    "Your mother is... okay, not great. I just didn't want to worry you more.",
    "I may have exaggerated a bit, honey. Still feeling it."
};

    [Header("Stable State - Truthful")]
    public List<string> stableTruthfulConversations = new List<string>
{
    "Hi sweetheart! It's Mom. I'm feeling completely healthy now thanks to you!",
    "Hello dear! Your mother {name}, {age} years old and fully recovered!",
    "You saved me, honey. I'm {name}, feeling wonderful thanks to my brilliant daughter!",
    "Hey baby! Mom's back to normal. You're an incredible nurse, I'm so proud!",
    "Hi sweetie! It's your {age}-year-old mother, feeling great thanks to you!",
    "Oh honey! I'm completely better now. You took such good care of your mom!",
    "Hi dear! Everything is perfect, no more problems. My daughter the hero!",
    "Hello baby! You can see your mom is all better now!",
    "Good to see you smiling again, honey! Mom's feeling fantastic thanks to you!",
    "Hello my dear! Your mother is all better and so grateful for you!"
};

    [Header("Stable State - Lying")]
    public List<string> stableLyingConversations = new List<string>
{
    "Hi honey... Mom might have pushed herself too hard, feeling a bit off again.",
    "Hi sweetie. Probably nothing, but your mother is feeling slightly unwell again.",
    "It's Mom. Everything's fine... mostly. Just a small concern, don't worry too much.",
    "Hello dear! I might have overdone it. Your old mom is feeling tired.",
    "I'm completely fine! Well... maybe a tiny bit of discomfort. Nothing for you to fuss about.",
    "Your mother may have been a bit too optimistic. Feeling a little off, honey.",
    "Don't tell anyone, but Mom's not feeling quite as good as I said earlier."
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