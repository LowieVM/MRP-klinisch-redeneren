using System;
using UnityEngine;

public class PatientStateManager : MonoBehaviour
{
    [Header("State Settings")]
    [SerializeField] private PatientState currentState = PatientState.Critical;
    [SerializeField] private int currentStage = 0;

    [Header("Healing Stages Configuration")]
    [SerializeField] private int totalStages = 4; // Critical -> Unstable -> Improving -> Stable
    [SerializeField] private bool autoProgressStages = false;
    [SerializeField] private float autoProgressDelay = 5f;

    [Header("Lying Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float lyingChanceCritical = 0.8f;
    [Range(0f, 1f)]
    [SerializeField] private float lyingChanceUnstable = 0.6f;
    [Range(0f, 1f)]
    [SerializeField] private float lyingChanceImproving = 0.4f;
    [Range(0f, 1f)]
    [SerializeField] private float lyingChanceStable = 0.2f;

    public event Action<PatientState> OnStateChanged;
    public event Action<int> OnStageChanged;
    public event Action<bool> OnTruthfulnessChecked;
    public event Action OnPatientFullyHealed;
    public event Action<string> OnMedicineApplied;

    private float lastProgressTime;

    private void Start()
    {
        SetState(currentState);
        lastProgressTime = Time.time;
    }

    private void Update()
    {
        if (autoProgressStages && currentState != PatientState.Stable)
        {
            if (Time.time - lastProgressTime >= autoProgressDelay)
            {
                ProgressStage();
                lastProgressTime = Time.time;
            }
        }
    }

    public void SetState(PatientState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
            Debug.Log($"Patient state changed to: {currentState}");

            // Reset stage when manually setting state
            currentStage = GetStageForState(currentState);
            OnStageChanged?.Invoke(currentStage);
        }
    }

    public PatientState GetCurrentState()
    {
        return currentState;
    }

    public int GetCurrentStage()
    {
        return currentStage;
    }

    public float GetHealingProgress()
    {
        return (float)currentStage / totalStages;
    }
    public bool ApplyMedicine(string medicineName = "Medicine")
    {
        if (currentState == PatientState.Stable)
        {
            Debug.Log("Patient is already stable, no medicine needed.");
            OnMedicineApplied?.Invoke($"{medicineName} (Patient already stable)");
            return false;
        }

        Debug.Log($"Applied {medicineName}. Patient progressing...");
        OnMedicineApplied?.Invoke(medicineName);

        ProgressStage();
        return true;
    }

    public void ProgressStage()
    {
        if (currentStage < totalStages)
        {
            currentStage++;
            OnStageChanged?.Invoke(currentStage);

            PatientState newState = GetStateForStage(currentStage);

            if (newState != currentState)
            {
                currentState = newState;
                OnStateChanged?.Invoke(currentState);
                Debug.Log($"Patient improved to: {currentState} (Stage {currentStage}/{totalStages})");
            }

            if (currentState == PatientState.Stable)
            {
                Debug.Log("Patient fully healed!");
                OnPatientFullyHealed?.Invoke();
            }
        }
        else
        {
            Debug.Log("Patient is already at maximum healing stage.");
        }
    }

    public void RegressStage(int amount = 1)
    {
        currentStage = Mathf.Max(0, currentStage - amount);
        OnStageChanged?.Invoke(currentStage);

        PatientState newState = GetStateForStage(currentStage);

        if (newState != currentState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
            Debug.Log($"Patient condition worsened to: {currentState} (Stage {currentStage}/{totalStages})");
        }
    }

    public void ResetPatient()
    {
        currentStage = 0;
        currentState = PatientState.Critical;
        OnStageChanged?.Invoke(currentStage);
        OnStateChanged?.Invoke(currentState);
        Debug.Log("Patient reset to critical condition.");
    }

    public bool IsTruthful()
    {
        float lyingChance = GetLyingChanceForCurrentState();
        bool truthful = UnityEngine.Random.value > lyingChance;
        OnTruthfulnessChecked?.Invoke(truthful);

        return truthful;
    }

    private float GetLyingChanceForCurrentState()
    {
        switch (currentState)
        {
            case PatientState.Critical:
                return lyingChanceCritical;
            case PatientState.Unstable:
                return lyingChanceUnstable;
            case PatientState.Improving:
                return lyingChanceImproving;
            case PatientState.Stable:
                return lyingChanceStable;
            default:
                return 0.5f;
        }
    }

    private PatientState GetStateForStage(int stage)
    {
        float progress = (float)stage / totalStages;

        if (progress >= 1f)
            return PatientState.Stable;
        else if (progress >= 0.66f)
            return PatientState.Improving;
        else if (progress >= 0.33f)
            return PatientState.Unstable;
        else
            return PatientState.Critical;
    }

    private int GetStageForState(PatientState state)
    {
        switch (state)
        {
            case PatientState.Critical:
                return 0;
            case PatientState.Unstable:
                return Mathf.RoundToInt(totalStages * 0.33f);
            case PatientState.Improving:
                return Mathf.RoundToInt(totalStages * 0.66f);
            case PatientState.Stable:
                return totalStages;
            default:
                return 0;
        }
    }

    [ContextMenu("Test: Apply Medicine")]
    private void TestApplyMedicine()
    {
        ApplyMedicine("Test Medicine");
    }

    [ContextMenu("Test: Regress Stage")]
    private void TestRegressStage()
    {
        RegressStage();
    }

    [ContextMenu("Test: Reset Patient")]
    private void TestResetPatient()
    {
        ResetPatient();
    }

    [ContextMenu("Test: Check Truthfulness")]
    private void TestTruthfulness()
    {
        bool truthful = IsTruthful();
        Debug.Log($"Patient is {(truthful ? "truthful" : "lying")}");
    }
}

public enum PatientState
{
    Critical,   
    Unstable,   
    Improving,  
    Stable     
}