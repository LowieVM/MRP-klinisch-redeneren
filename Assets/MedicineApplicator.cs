using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Example script showing how to apply medicine when interacting with an object
/// Attach this to medicine bottles, syringes, or treatment equipment
/// </summary>
public class MedicineApplicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PatientStateManager patientStateManager;

    [Header("Medicine Settings")]
    [SerializeField] private string medicineName = "Painkiller";
    [SerializeField] private bool isCorrectTreatment = true;
    [SerializeField] private int healingAmount = 1; // How many stages this medicine heals

    [Header("Feedback")]
    [SerializeField] private AudioClip applicationSound;
    [SerializeField] private ParticleSystem applicationEffect;

    private XRGrabInteractable grabInteractable;
    private AudioSource audioSource;

    private void Start()
    {
        // Find patient state manager if not assigned
        if (patientStateManager == null)
        {
            patientStateManager = FindObjectOfType<PatientStateManager>();
        }

        // Setup XR interaction
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.activated.AddListener(OnMedicineActivated);
        }

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && applicationSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Subscribe to patient events
        if (patientStateManager != null)
        {
            patientStateManager.OnStateChanged += OnPatientStateChanged;
            patientStateManager.OnPatientFullyHealed += OnPatientFullyHealed;
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(OnMedicineActivated);
        }

        if (patientStateManager != null)
        {
            patientStateManager.OnStateChanged -= OnPatientStateChanged;
            patientStateManager.OnPatientFullyHealed -= OnPatientFullyHealed;
        }
    }

    private void OnMedicineActivated(ActivateEventArgs args)
    {
        ApplyMedicine();
    }

    public void ApplyMedicine()
    {
        if (patientStateManager == null)
        {
            Debug.LogError("PatientStateManager not found!");
            return;
        }

        if (isCorrectTreatment)
        {
            // Correct treatment - heal patient
            for (int i = 0; i < healingAmount; i++)
            {
                patientStateManager.ApplyMedicine(medicineName);
            }

            Debug.Log($"✓ Applied {medicineName} successfully!");
            PlayFeedback(true);
        }
        else
        {
            // Wrong treatment - patient condition worsens
            patientStateManager.RegressStage(healingAmount);
            Debug.Log($"✗ {medicineName} was incorrect treatment! Patient condition worsened.");
            PlayFeedback(false);
        }
    }

    private void PlayFeedback(bool success)
    {
        // Play sound
        if (audioSource != null && applicationSound != null)
        {
            audioSource.PlayOneShot(applicationSound);
        }

        // Play particle effect
        if (applicationEffect != null)
        {
            applicationEffect.Play();
        }

        // Could add haptic feedback here for VR controllers
    }

    private void OnPatientStateChanged(PatientState newState)
    {
        Debug.Log($"Medicine Applicator noticed: Patient is now {newState}");
    }

    private void OnPatientFullyHealed()
    {
        Debug.Log($"Medicine Applicator: Patient fully healed with {medicineName}!");
    }

    // Optional: Visual feedback method for UI
    public string GetTreatmentInfo()
    {
        if (patientStateManager == null) return "No patient data";

        string currentState = patientStateManager.GetCurrentState().ToString();
        int stage = patientStateManager.GetCurrentStage();
        float progress = patientStateManager.GetHealingProgress() * 100f;

        return $"{medicineName}\nPatient: {currentState}\nProgress: {progress:F0}% (Stage {stage})";
    }
}